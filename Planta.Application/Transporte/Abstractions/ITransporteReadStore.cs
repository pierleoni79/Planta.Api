// Ruta: /Planta.Application/Transporte/Abstractions/ITransporteReadStore.cs | V1.1-fix (docs + defaults + contrato claro)
#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Planta.Contracts.Transporte;

namespace Planta.Application.Transporte.Abstractions;

/// <summary>
/// Puerta de lectura para resolver información de transporte (sólo lecturas).
/// No realiza escrituras ni efectos colaterales.
/// </summary>
public interface ITransporteReadStore
{
    /// <summary>
    /// Lista los vehículos marcados como “favoritos” para una empresa.
    /// </summary>
    /// <param name="empresaId">Id de la empresa propietaria del catálogo de favoritos.</param>
    /// <param name="max">
    /// Máximo de elementos a devolver (1..1000). Implementación debe capar valores inválidos.
    /// </param>
    /// <param name="ct">Token de cancelación (opcional).</param>
    Task<IReadOnlyList<VehiculoFavoritoDto>> ListarFavoritosAsync(
    int empresaId,
    int max = 50,
    CancellationToken ct = default);

    /// <summary>
    /// Resuelve datos de transporte a partir de la placa (p. ej. vehículo, conductor, estado).
    /// </summary>
    /// <param name="placa">
    /// Placa a resolver. Sugerido normalizar a <c>Trim().ToUpperInvariant()</c> en la implementación.
    /// </param>
    /// <param name="ct">Token de cancelación (opcional).</param>
    /// <returns>
    /// <see cref="TransporteResolucionDto"/> cuando se encuentra; <c>null</c> si no existe.
    /// </returns>
    Task<TransporteResolucionDto?> ResolverTransportePorPlacaAsync(
        string placa,
        CancellationToken ct = default);


}