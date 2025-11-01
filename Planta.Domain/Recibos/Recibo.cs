// Ruta: /Planta.Domain/Recibos/Recibo.cs | V1.2
#nullable enable
namespace Planta.Domain.Recibos;

using Planta.Domain.Common;
using Planta.Domain.Enums;

public sealed class Recibo : AggregateRoot<Guid>, IAuditable, IConcurrencySafe
{
    // Auditoría (en BD: datetimeoffset)
    public DateTimeOffset FechaCreacionUtc { get; private set; }
    public DateTimeOffset? UltimaActualizacionUtc { get; private set; }

    // Maestros
    public int EmpresaId { get; private set; }
    public int? PlantaId { get; private set; }
    public int? AlmacenOrigenId { get; private set; }
    public int? ClienteId { get; private set; }
    public int? VehiculoId { get; private set; }
    public int? ConductorId { get; private set; }
    public int? MaterialId { get; private set; }

    // Proceso (en BD: tinyint)
    public int Estado { get; private set; }
    public int DestinoTipo { get; private set; }
    public decimal Cantidad { get; private set; }
    public string? Observaciones { get; private set; }

    // Snapshots
    public string? PlacaSnapshot { get; private set; }
    public string? ConductorNombreSnapshot { get; private set; }

    // Documentos / idempotencia
    public string? ReciboFisicoNumero { get; private set; }
    public string? NumeroGenerado { get; private set; }
    public string? IdempotencyKey { get; private set; }

    private readonly List<ReciboEstadoLog> _historial = new();
    public IReadOnlyCollection<ReciboEstadoLog> Historial => _historial;

    private Recibo() : base(Guid.Empty) { }

    private Recibo(Guid id, int empresaId, int estado, int destinoTipo, decimal cantidad, DateTimeOffset nowUtc) : base(id)
    {
        EmpresaId = empresaId;
        Estado = estado;
        DestinoTipo = destinoTipo;
        Cantidad = cantidad;
        FechaCreacionUtc = nowUtc;
        UltimaActualizacionUtc = nowUtc;
    }

    public static Recibo CreateNew(Guid id, int empresaId, DestinoTipo destino, decimal cantidad, DateTimeOffset nowUtc, string? idempotencyKey = null)
    {
        Guard.AgainstNonPositive(cantidad, nameof(cantidad));
        var r = new Recibo(id, empresaId, ReciboEstado.EnTransitoPlanta.Id, destino.Id, cantidad, nowUtc)
        { IdempotencyKey = idempotencyKey };
        r.Raise(new ReciboCreadoEvent(r));
        r.AppendEstadoLog(r.Estado, "Creación");
        return r;
    }

    public void SetSnapshots(string? placa, string? conductorNombre)
    { PlacaSnapshot = placa; ConductorNombreSnapshot = conductorNombre; Touch(); }

    public void AsignarDocumentos(string? reciboFisico, string? numeroGenerado)
    {
        if (!string.IsNullOrWhiteSpace(NumeroGenerado) && numeroGenerado != NumeroGenerado)
            throw new InvalidOperationException("NumeroGenerado ya asignado");
        ReciboFisicoNumero = reciboFisico?.Trim();
        NumeroGenerado = numeroGenerado?.Trim();
        Touch();
    }

    public void CambiarEstado(ReciboEstado nuevo, string? observacion = null)
    {
        if (!EsTransicionValida(Estado, nuevo.Id))
            throw new InvalidOperationException($"Transición de estado no válida: {Estado} → {nuevo.Id}");
        var anterior = Estado;
        Estado = nuevo.Id;
        AppendEstadoLog(Estado, observacion);
        Raise(new ReciboEstadoCambiadoEvent(Id, anterior, Estado));
        Touch();
    }

    public void RegistrarMaterial(int materialId, decimal cantidad)
    { Guard.AgainstNonPositive(cantidad, nameof(cantidad)); MaterialId = materialId; Cantidad = cantidad; Touch(); }

    public void VincularTransporte(int? vehiculoId, int? conductorId, string? placaSnap = null, string? conductorSnap = null)
    { VehiculoId = vehiculoId; ConductorId = conductorId; SetSnapshots(placaSnap, conductorSnap); }

    public void VincularOrigen(int? plantaId, int? almacenOrigenId, int? clienteId)
    { PlantaId = plantaId; AlmacenOrigenId = almacenOrigenId; ClienteId = clienteId; Touch(); }

    public string ComputeWeakETag()
    {
        var ticks = (UltimaActualizacionUtc ?? FechaCreacionUtc).UtcTicks;
        return $"W/\"{Id:N}-{ticks}\"";
    }

    private void AppendEstadoLog(int estado, string? obs)
    { _historial.Add(new ReciboEstadoLog(0, this.Id, estado, DateTimeOffset.UtcNow, obs, null)); }

    private static bool EsTransicionValida(int from, int to) =>
        (from, to) switch
        {
            (10, 12) => true,
            (12, 20) => true,
            (20, 30) => true,
            (10 or 12 or 20, 90) => true,
            (_, 99) => true,
            _ => false
        };

    private void Touch() => UltimaActualizacionUtc = DateTimeOffset.UtcNow;
}
