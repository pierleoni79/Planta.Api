// Ruta: /Planta.Application/Transporte/Queries/ResolverTransportePorPlaca/ResolverTransportePorPlacaValidator.cs | V1.0
#nullable enable
using FluentValidation;

namespace Planta.Application.Transporte.Queries.ResolverTransportePorPlaca;

public sealed class ResolverTransportePorPlacaValidator : AbstractValidator<ResolverTransportePorPlacaQuery>
{
    public ResolverTransportePorPlacaValidator()
    {
        RuleFor(x => x.Placa)
            .NotEmpty().WithMessage("La placa es requerida.")
            .MaximumLength(10);
    }
}
