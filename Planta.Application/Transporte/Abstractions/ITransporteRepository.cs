// Ruta: /Planta.Application/Transporte/Abstractions/ITransporteRepository.cs | V1.2
#nullable enable
using System.Threading;

namespace Planta.Application.Transporte.Abstractions;

/// <summary>
/// Operaciones de escritura relacionadas con Transporte (vehículos / conductor).
/// </summary>
public interface ITransporteRepository
{
    /// <summary>Marca o desmarca un vehículo como favorito.</summary>
    Task<bool> ToggleFavoritoAsync(int vehiculoId, bool esFavorito, CancellationToken ct = default);

    /// <summary>
    /// Garantiza una asignación abierta Vehículo–Conductor.
    /// Cierra cualquier asignación abierta para el vehículo con otro conductor (Hasta = now)
    /// y abre (si no existe) la asignación con <paramref name="conductorId"/> (Desde = now, Hasta = NULL).
    /// Idempotente si ya está asignado el mismo conductor y sigue abierto.
    /// </summary>
    Task EnsureVehiculoConductorAsync(int vehiculoId, int conductorId, CancellationToken ct = default);
}
