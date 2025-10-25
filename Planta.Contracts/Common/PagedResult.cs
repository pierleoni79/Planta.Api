// Ruta: /Planta.Contracts/Common/PagedResult.cs | V1.1
namespace Planta.Contracts.Common;

public sealed record PagedResult<T>(
IReadOnlyList<T> Items,
int Page,
int PageSize,
int Total
)
{
    public int Count => Items?.Count ?? 0;
    public int TotalPages => PageSize <= 0 ? 0 : (int)System.Math.Ceiling((double)Total / PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}