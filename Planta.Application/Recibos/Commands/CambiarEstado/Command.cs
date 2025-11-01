// Ruta: /Planta.Application/Recibos/Commands/CambiarEstado/Command.cs | V1.2 (fix)
#nullable enable
using Planta.Application.Common.Exceptions;
using Planta.Contracts.Recibos;
using Planta.Contracts.Recibos.Requests;

// ALIAS para evitar ambigüedad:
using C_ReciboEstado = Planta.Contracts.Enums.ReciboEstado;
using D_ReciboEstado = Planta.Domain.Enums.ReciboEstado;

namespace Planta.Application.Recibos.Commands.CambiarEstado;

public sealed record Command(CambiarEstadoRequest Value) : IRequest<ReciboDto>;

public sealed class Handler : IRequestHandler<Command, ReciboDto>
{
    private readonly IReciboRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public Handler(IReciboRepository repo, IUnitOfWork uow, IMapper mapper)
    { _repo = repo; _uow = uow; _mapper = mapper; }

    public async Task<ReciboDto> Handle(Command request, CancellationToken ct)
    {
        var r = request.Value;
        var entity = await _repo.GetAsync(r.Id, ct) ?? throw new NotFoundException($"Recibo {r.Id} no encontrado");

        // Concurrencia optimista por ETag (si viene If-Match)
        if (!string.IsNullOrWhiteSpace(r.IfMatchETag))
        {
            var current = entity.ComputeWeakETag();
            if (!string.Equals(current, r.IfMatchETag, StringComparison.Ordinal))
                throw new ConcurrencyException("ETag no coincide. Actualiza y reintenta.");
        }

        // Contracts -> Domain (sin ambigüedad)
        var nuevo = r.NuevoEstado switch
        {
            C_ReciboEstado.EnTransitoPlanta => D_ReciboEstado.EnTransitoPlanta,
            C_ReciboEstado.Descargando => D_ReciboEstado.Descargando,
            C_ReciboEstado.Procesado => D_ReciboEstado.Procesado,
            C_ReciboEstado.Cerrado => D_ReciboEstado.Cerrado,
            C_ReciboEstado.Anulado => D_ReciboEstado.Anulado,
            C_ReciboEstado.Error => D_ReciboEstado.Error,
            _ => throw new ValidationException("Estado inválido")
        };

        entity.CambiarEstado(nuevo, r.Observacion);

        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ReciboDto>(entity);
    }
}
