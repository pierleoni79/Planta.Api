// Ruta: /Planta.Contracts/Common/PagedRequest.cs | V1.1
namespace Planta.Contracts.Common;

public sealed class PagedRequest
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 200;

    private int _page = DefaultPage;
    private int _pageSize = DefaultPageSize;

    public int Page
    {
        get => _page;
        init => _page = value < 1 ? DefaultPage : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init
        {
            if (value < 1) _pageSize = DefaultPageSize;
            else _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }

    // Usamos "q" para alinear con los endpoints (?q=)
    public string? Q { get; init; }


}