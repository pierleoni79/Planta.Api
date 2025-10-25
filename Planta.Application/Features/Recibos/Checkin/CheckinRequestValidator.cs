// Ruta: /Planta.Application/Features/Recibos/Checkin/CheckinRequestValidator.cs | V1.0
using FluentValidation;
using Planta.Contracts.Recibos;

namespace Planta.Application.Features.Recibos.Checkin;

public sealed class CheckinRequestValidator : AbstractValidator<CheckinRequest>
{
    public CheckinRequestValidator()
    {
        RuleFor(x => x.Usuario).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Observaciones).MaximumLength(500);
    }
}
