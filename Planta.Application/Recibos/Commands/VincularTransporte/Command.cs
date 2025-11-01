// Ruta: /Planta.Application/Recibos/Commands/VincularTransporte/Command.cs | V1.1
#nullable enable
using Planta.Application.Common.Exceptions;
using Planta.Contracts.Recibos;
using Planta.Contracts.Recibos.Requests;

namespace Planta.Application.Recibos.Commands.VincularTransporte;

public sealed record Command(VincularTransporteRequest Value) : IRequest<ReciboDto>;

public sealed class Handler : IRequestHandler<Command, ReciboDto>
{
    private readonly IReciboRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public Handler(IReciboRepository repo, IUnitOfWork uow, IMapper mapper)
    { _repo = repo; _uow = uow; _mapper = mapper; }

    public async Task<ReciboDto> Handle(Command request, CancellationToken ct)
    {
        var v = request.Value;
        var entity = await _repo.GetAsync(v.Id, ct) ?? throw new NotFoundException($"Recibo {v.Id} no encontrado");
        entity.VincularTransporte(v.VehiculoId, v.ConductorId, v.PlacaSnapshot, v.ConductorNombreSnapshot);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ReciboDto>(entity);
    }
}
