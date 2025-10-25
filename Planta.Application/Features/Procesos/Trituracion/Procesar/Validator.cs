// Ruta: /Planta.Application/Features/Procesos/Trituracion/Procesar/Validator.cs | V1.0
using FluentValidation;

namespace Planta.Application.Features.Procesos.Trituracion.Procesar;

public sealed class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.ReciboId).NotEmpty();
        RuleFor(x => x.Body.PesoEntrada).GreaterThan(0);
        RuleFor(x => x.Body.PesoSalida).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Body.Residuos).GreaterThanOrEqualTo(0);
    }
}
