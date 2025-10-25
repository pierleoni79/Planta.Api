using MediatR;
using Planta.Contracts.Procesos;

namespace Planta.Application.Features.Procesos.RegistrarTrituracion
{
    public sealed record Command(Guid ReciboId, ProcesarTrituracionRequest Body) : IRequest<ProcesoResultDto>;
}
