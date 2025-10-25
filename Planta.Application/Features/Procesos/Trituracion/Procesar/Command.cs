// Ruta: /Planta.Application/Features/Procesos/Trituracion/Procesar/Command.cs | V1.0
using MediatR;
using Planta.Contracts.Procesos;

namespace Planta.Application.Features.Procesos.Trituracion.Procesar;

public sealed record Command(Guid ReciboId, ProcesarTrituracionRequest Body)
    : IRequest<ProcesoResultDto>;
