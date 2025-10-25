// Ruta: /Planta.Application/Features/Recibos/Checkin/Command.cs | V1.0
using System;
using MediatR;
using Planta.Contracts.Recibos;

namespace Planta.Application.Features.Recibos.Checkin
{
    public sealed record Command(Guid ReciboId, CheckinRequest Body) : IRequest<Unit>;
}
