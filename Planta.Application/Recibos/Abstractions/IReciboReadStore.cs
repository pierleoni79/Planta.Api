// Ruta: /Planta.Application/Recibos/Abstractions/IReciboReadStore.cs | V1.1
#nullable enable
using Planta.Contracts.Common;
using Planta.Contracts.Recibos;
using Planta.Contracts.Recibos.Queries;

namespace Planta.Application.Recibos.Abstractions;

/// <summary>
/// Read-store optimizado para listados/filtros/paginación de Recibos.
/// Implementación en Infrastructure (Dapper/EF/Views).
/// </summary>
public interface IReciboReadStore
{
    Task<PagedResult<ReciboListItemDto>> ListAsync(ReciboListQuery query, CancellationToken ct);
    Task<ReciboDto?> GetByIdAsync(Guid id, CancellationToken ct);
}
