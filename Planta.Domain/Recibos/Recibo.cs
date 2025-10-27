#nullable enable
using Planta.Domain.Produccion;
using Planta.Domain.Shared;

namespace Planta.Domain.Recibos;

public sealed class Recibo
{
    public Guid Id { get; private set; }
    public int Consecutivo { get; private set; }
    public DateTimeOffset FechaCreacion { get; private set; }

    // FK y snapshots según BD
    public int EmpresaId { get; private set; }
    public int VehiculoId { get; private set; }
    public string? PlacaSnapshot { get; private set; }
    public int? ConductorId { get; private set; }
    public string? ConductorNombreSnapshot { get; private set; }
    public int? ClienteId { get; private set; }
    public int MaterialId { get; private set; }
    public int AlmacenOrigenId { get; private set; }

    public byte DestinoTipo { get; private set; }        // tinyint
    public ReciboEstado Estado { get; private set; }     // tinyint (enum byte)

    public string UsuarioCreador { get; private set; } = string.Empty; // NOT NULL
    public string? Observaciones { get; private set; }                 // NULL

    public DateTimeOffset? UltimaActualizacion { get; private set; }   // NULL

    public string? ReciboFisicoNumero { get; private set; }
    public string? NumeroGenerado { get; private set; }
    public bool AutoGenerado { get; private set; }
    public DateTimeOffset? AutoGeneradoEn { get; private set; }
    public bool Activo { get; private set; } = true;

    public decimal Cantidad { get; private set; } // decimal(18,3) NOT NULL

    public string? ReciboFisicoNumeroNorm { get; private set; }

    // Idempotencia
    public string? IdempotencyKey { get; private set; } // "scope:key"

    // Navegación proceso (si aún no mapeas, EF la puede ignorar)
    public ProcesoTrituracion? ProcesoFinal { get; private set; }

    private Recibo() { } // EF

    public static Recibo CrearNuevo(
        int empresaId, int vehiculoId, int materialId, int almacenOrigenId,
        int? clienteId, int? conductorId, string? placaSnapshot, string? conductorNombreSnapshot,
        byte destinoTipo, string? obsInicial, decimal cantidad,
        IdempotencyKey idemKey, string usuarioCreador)
    {
        if (string.IsNullOrWhiteSpace(usuarioCreador))
            throw new ArgumentException("UsuarioCreador es requerido.", nameof(usuarioCreador));

        return new Recibo
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            VehiculoId = vehiculoId,
            MaterialId = materialId,
            AlmacenOrigenId = almacenOrigenId,
            ClienteId = clienteId,
            ConductorId = conductorId,
            PlacaSnapshot = placaSnapshot,
            ConductorNombreSnapshot = conductorNombreSnapshot,
            DestinoTipo = destinoTipo,
            Estado = ReciboEstado.EnTransito_Planta,
            UsuarioCreador = usuarioCreador,
            Observaciones = BuildObs("[Creación]", obsInicial),
            FechaCreacion = DateTimeOffset.UtcNow,
            UltimaActualizacion = null,
            Cantidad = cantidad, // non-nullable
            IdempotencyKey = idemKey.ToString()
        };
    }

    public void TouchNow() => UltimaActualizacion = DateTimeOffset.UtcNow;

    private static string BuildObs(string prefix, string? extra)
        => $"{prefix} {extra ?? ""}".Trim();
}
