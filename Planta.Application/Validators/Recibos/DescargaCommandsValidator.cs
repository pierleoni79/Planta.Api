// Ruta: /Planta.Application/Validators/Recibos/DescargaCommandsValidator.cs | V1.0
#nullable enable
using FluentValidation;
using Planta.Application.Features.Recibos;

namespace Planta.Application.Validators.Recibos;

public sealed class DescargaInicioCommandValidator : AbstractValidator<DescargaInicioCommand>
{
    public DescargaInicioCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.IdempotencyKey).MaximumLength(128).When(x => !string.IsNullOrWhiteSpace(x.IdempotencyKey));
    }
}

public sealed class DescargaFinCommandValidator : AbstractValidator<DescargaFinCommand>
{
    public DescargaFinCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.IdempotencyKey).MaximumLength(128).When(x => !string.IsNullOrWhiteSpace(x.IdempotencyKey));
    }
}
