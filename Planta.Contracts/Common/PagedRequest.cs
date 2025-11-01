// Ruta: /Planta.Contracts/Common/PagedRequest.cs | V1.1
#nullable enable
namespace Planta.Contracts.Common;

public sealed record PagedRequest(
    int Page = 1,
    int PageSize = 20,
    IReadOnlyList<SortSpec>? Sort = null
);
