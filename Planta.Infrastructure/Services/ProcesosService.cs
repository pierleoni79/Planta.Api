// Ruta: /Planta.Infrastructure/Services/ProcesosService.cs | V1.0
using MediatR;
using Planta.Application.Abstractions;
using Planta.Application.Features.Procesos.Trituracion.Procesar; // Command
using Planta.Contracts.Procesos;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Planta.Infrastructure.Services
{
    public sealed class ProcesosService : IProcesosService
    {
        private readonly IMediator _mediator;
        public ProcesosService(IMediator mediator) => _mediator = mediator;

        public Task<ProcesoResultDto> ProcesarTrituracionAsync(Guid reciboId, ProcesarTrituracionRequest body, CancellationToken ct)
            => _mediator.Send(new Command(reciboId, body), ct);
    }
}
