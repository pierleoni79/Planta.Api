// Ruta: /Planta.Application/Recibos/Queries/ListarRecibos/Query.cs | V1.1
#nullable enable
using Planta.Application.Recibos.Abstractions;
using Planta.Contracts.Common;
using Planta.Contracts.Recibos;
using Planta.Contracts.Recibos.Queries;

namespace Planta.Application.Recibos.Queries.ListarRecibos;

public sealed record Query(ReciboListQuery Value) : IRequest<PagedResult<ReciboListItemDto>>;

public sealed class Handler : IRequestHandler<Query, PagedResult<ReciboListItemDto>>
{
    private readonly IReciboReadStore _read;

    public Handler(IReciboReadStore read) => _read = read;

    public Task<PagedResult<ReciboListItemDto>> Handle(Query request, CancellationToken ct)
        => _read.ListAsync(request.Value, ct);
}
