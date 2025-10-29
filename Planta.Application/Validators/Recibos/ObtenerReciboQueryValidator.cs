// Ruta: /Planta.Application/Validators/Recibos/ObtenerReciboQueryValidator.cs | V1.0
#nullable enable
using FluentValidation;
using Planta.Application.Features.Recibos;

namespace Planta.Application.Validators.Recibos;

public sealed class ObtenerReciboQueryValidator : AbstractValidator<ObtenerReciboQuery>
{
    public ObtenerReciboQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
