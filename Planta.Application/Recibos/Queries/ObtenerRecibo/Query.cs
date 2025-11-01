// Ruta: /Planta.Application/Recibos/Queries/ObtenerRecibo/Query.cs | V1.1
#nullable enable
using Planta.Application.Common.Exceptions;
using Planta.Application.Recibos.Abstractions;
using Planta.Contracts.Recibos;

namespace Planta.Application.Recibos.Queries.ObtenerRecibo;

public sealed record Query(Guid Id) : IRequest<ReciboDto>;

public sealed class Handler : IRequestHandler<Query, ReciboDto>
{
    private readonly IReciboReadStore _read;

    public Handler(IReciboReadStore read) => _read = read;

    public async Task<ReciboDto> Handle(Query request, CancellationToken ct)
    {
        var dto = await _read.GetByIdAsync(request.Id, ct);
        if (dto is null) throw new NotFoundException($"Recibo {request.Id} no encontrado");
        return dto;
    }
}
