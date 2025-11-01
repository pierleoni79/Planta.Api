// Ruta: /Planta.Application/Recibos/Commands/ActualizarRecibo/Command.cs | V1.1
#nullable enable
using Planta.Application.Common.Abstractions;
using Planta.Application.Common.Exceptions;
using Planta.Contracts.Recibos;
using Planta.Contracts.Recibos.Requests;

namespace Planta.Application.Recibos.Commands.ActualizarRecibo;

public sealed record Command(ReciboUpdateRequest Value) : IRequest<ReciboDto>;

public sealed class Handler : IRequestHandler<Command, ReciboDto>
{
    private readonly IReciboRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IClock _clock;

    public Handler(IReciboRepository repo, IUnitOfWork uow, IMapper mapper, IClock clock)
    { _repo = repo; _uow = uow; _mapper = mapper; _clock = clock; }

    public async Task<ReciboDto> Handle(Command request, CancellationToken ct)
    {
        var v = request.Value;
        var entity = await _repo.GetAsync(v.Id, ct);
        if (entity is null) throw new NotFoundException($"Recibo {v.Id} no encontrado");

        if (v.Observaciones is not null)
            entity.GetType().GetProperty("Observaciones")?.SetValue(entity, v.Observaciones);

        if (v.ReciboFisicoNumero is not null || v.NumeroGenerado is not null)
            entity.AsignarDocumentos(v.ReciboFisicoNumero, v.NumeroGenerado);

        // touch
        entity.GetType().GetMethod("VincularOrigen")?.Invoke(entity, new object?[] { entity.PlantaId, entity.AlmacenOrigenId, entity.ClienteId });

        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ReciboDto>(entity);
    }
}
