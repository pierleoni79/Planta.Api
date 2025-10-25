// Ruta: /Planta.Infrastructure/Services/ProcesosService.cs | V1.2
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Planta.Application.Abstractions;
using Planta.Contracts.Procesos;

// 👇 Alias correcto a la feature según Planta.Application
using RegistrarCmd = Planta.Application.Features.Procesos.RegistrarTrituracion.Command;

namespace Planta.Infrastructure.Services
{
    public sealed class ProcesosService : IProcesosService
    {
        private readonly IMediator _mediator;
        public ProcesosService(IMediator mediator) => _mediator = mediator;

        public Task<ProcesoResultDto> ProcesarTrituracionAsync(Guid reciboId, ProcesarTrituracionRequest body, CancellationToken ct)
            => _mediator.Send(new RegistrarCmd(reciboId, body), ct);
    }
}
