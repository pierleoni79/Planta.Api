// Ruta: /Planta.Application/Validators/Recibos/CheckInCommandValidator.cs | V1.0
#nullable enable
using FluentValidation;
using Planta.Application.Features.Recibos;

namespace Planta.Application.Validators.Recibos;

public sealed class CheckInCommandValidator : AbstractValidator<CheckInCommand>
{
    public CheckInCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.IfMatch).NotEmpty();
        RuleFor(x => x.ETag).NotEmpty();
        RuleFor(x => x.IdempotencyKey).MaximumLength(128).When(x => !string.IsNullOrWhiteSpace(x.IdempotencyKey));
    }
}
