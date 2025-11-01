// Ruta: /Planta.Application/Recibos/Commands/CrearRecibo/Command.cs | V1.3
#nullable enable
using AutoMapper;
using MediatR;

using Planta.Application.Common.Abstractions;
using Planta.Application.Common.Exceptions;
using Planta.Application.Transporte.Abstractions;

using Planta.Contracts.Recibos;
using Planta.Contracts.Recibos.Requests;

// Alias para evitar ambigüedad entre enums en Contracts y Domain
using C_DestinoTipo = Planta.Contracts.Enums.DestinoTipo;
using D_DestinoTipo = Planta.Domain.Enums.DestinoTipo;

using Planta.Domain.Recibos;

namespace Planta.Application.Recibos.Commands.CrearRecibo;

public sealed record Command(ReciboCreateRequest Value) : IRequest<ReciboDto>;

public sealed class Handler : IRequestHandler<Command, ReciboDto>
{
    private readonly IReciboRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IClock _clock;
    private readonly ITransporteRepository _transporteRepo;

    public Handler(
        IReciboRepository repo,
        IUnitOfWork uow,
        IMapper mapper,
        IClock clock,
        ITransporteRepository transporteRepo)
    {
        _repo = repo;
        _uow = uow;
        _mapper = mapper;
        _clock = clock;
        _transporteRepo = transporteRepo;
    }

    public async Task<ReciboDto> Handle(Command request, CancellationToken ct)
    {
        var r = request.Value;

        // Map explícito Contracts -> Domain
        var destinoDom = r.DestinoTipo switch
        {
            C_DestinoTipo.Planta => D_DestinoTipo.Planta,
            C_DestinoTipo.ClienteDirecto => D_DestinoTipo.ClienteDirecto,
            _ => throw new ValidationException("DestinoTipo inválido.")
        };

        // Crear entidad base (según tu Domain)
        var entity = Recibo.CreateNew(
            r.Id,
            r.EmpresaId,
            destinoDom,
            r.Cantidad,
            _clock.UtcNow,
            r.IdempotencyKey
        );

        // Snapshots opcionales
        if (!string.IsNullOrWhiteSpace(r.PlacaSnapshot) || !string.IsNullOrWhiteSpace(r.ConductorNombreSnapshot))
            entity.SetSnapshots(r.PlacaSnapshot, r.ConductorNombreSnapshot);

        if (!string.IsNullOrWhiteSpace(r.ReciboFisicoNumero))
            entity.AsignarDocumentos(r.ReciboFisicoNumero, null);

        // Persistir Recibo
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        // ⇩ Asegurar vínculo Vehículo–Conductor si ambos vienen en el request
        //    Cierra cualquier vínculo abierto con otro conductor y abre (o mantiene) el actual.
        if (r.VehiculoId is int vehId && r.ConductorId is int condId)
        {
            await _transporteRepo.EnsureVehiculoConductorAsync(vehId, condId, ct);
        }

        // Devolver DTO
        return _mapper.Map<ReciboDto>(entity);
    }
}
