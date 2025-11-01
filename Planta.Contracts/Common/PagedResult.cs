// Ruta: /Planta.Contracts/Common/PagedResult.cs | V1.1
#nullable enable
namespace Planta.Contracts.Common;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
