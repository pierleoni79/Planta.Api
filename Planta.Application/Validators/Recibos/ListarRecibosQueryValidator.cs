// Ruta: /Planta.Application/Validators/Recibos/ListarRecibosQueryValidator.cs | V1.0
#nullable enable
using FluentValidation;
using Planta.Application.Features.Recibos;

namespace Planta.Application.Validators.Recibos;

public sealed class ListarRecibosQueryValidator : AbstractValidator<ListarRecibosQuery>
{
    public ListarRecibosQueryValidator()
    {
        RuleFor(x => x.Page).SetValidator(new Common.PagedRequestValidator());
        RuleFor(x => x.EmpresaId).GreaterThan(0).When(x => x.EmpresaId.HasValue);
        RuleFor(x => x.ClienteId).GreaterThan(0).When(x => x.ClienteId.HasValue);
        RuleFor(x => x.Hasta)
            .GreaterThan(x => x.Desde!.Value)
            .When(x => x.Desde.HasValue && x.Hasta.HasValue);
    }
}
