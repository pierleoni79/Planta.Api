// Ruta: /Planta.Contracts/Recibos/CrearReciboRequest.cs | V1.2
#nullable enable
namespace Planta.Contracts.Recibos;

public sealed class CrearReciboRequest
{
    // FK obligatorios según BD
    public int EmpresaId { get; init; }
    public int VehiculoId { get; init; }
    public int MaterialId { get; init; }

    // Cliente puede ser NULL en BD → opcional aquí
    public int? ClienteId { get; init; }

    // Almacén es NOT NULL en BD → hazlo obligatorio
    public int AlmacenOrigenId { get; init; }

    // Snapshots opcionales (BD permite NULL)
    public string? PlacaSnapshot { get; init; }
    public int? ConductorId { get; init; }
    public string? ConductorNombreSnapshot { get; init; }

    // tinyint en BD → enum con byte subyacente en Contracts
    public DestinoTipo Destino { get; init; } // byte

    // Otros
    public string? ObservacionesIniciales { get; init; }
    public decimal? Cantidad { get; init; } // op.Recibo.Cantidad (decimal(18,3)) opcional

    // Idempotencia
    public string IdempotencyScope { get; init; } = "create";
    public string IdempotencyKey { get; init; } = string.Empty;
}
