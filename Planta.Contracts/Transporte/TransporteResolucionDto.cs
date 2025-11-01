// Ruta: /Planta.Contracts/Transporte/TransporteResolucionDto.cs | V1.2-fix (POCO + TieneHistorial + ClaseNombre)
#nullable enable
namespace Planta.Contracts.Transporte;

/// <summary>
/// Resultado de resolver transporte por placa:
/// Vehículo + Clase/Capacidad + Conductor (fuente: Historial/Recibo/Staging).
/// </summary>
public sealed class TransporteResolucionDto
{
    // ---- Vehículo ----
    public int VehiculoId { get; init; }
    public string Placa { get; init; } = default!;
    public int? ClaseVehiculoId { get; init; }
    public string? ClaseNombre { get; init; }
    public decimal? CapacidadM3 { get; init; }
    public bool VehiculoActivo { get; init; }

    // ---- Conductor (vigente si hay historial abierto) ----
    public int? ConductorId { get; init; }
    public string? ConductorNombreSnapshot { get; init; }
    public bool? ConductorActivo { get; init; }

    /// <summary>"Historial" | "Recibo" | "Staging" | "SinDatos"</summary>
    public string? FuenteResolucion { get; init; }

    /// <summary>
    /// True si existe cualquier registro (abierto o cerrado) en tpt.VehiculoConductorHist.
    /// </summary>
    public bool TieneHistorial { get; init; }
}
