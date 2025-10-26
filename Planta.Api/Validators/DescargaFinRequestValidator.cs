// Ruta: /Planta.Api/Validators/DescargaFinRequestValidator.cs | V1.0
using FluentValidation;
using Planta.Contracts.Recibos;

namespace Planta.Api.Validators;

public sealed class DescargaFinRequestValidator : AbstractValidator<DescargaFinRequest>
{
    public DescargaFinRequestValidator()
    {
        RuleFor(x => x.Usuario).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty().Must(k => k.StartsWith("descarga-fin:"))
            .WithMessage("IdempotencyKey debe iniciar con 'descarga-fin:'");

        RuleFor(x => x.Proceso).NotNull();
        RuleFor(x => x.Proceso.RecetaId).GreaterThan(0);
        RuleFor(x => x.Proceso.Entrada).NotNull();
        RuleFor(x => x.Proceso.Entrada.Cantidad).GreaterThan(0);
        RuleFor(x => x.Proceso.Entrada.Unidad).NotEmpty().Must(u => u.Equals("m3", StringComparison.OrdinalIgnoreCase))
            .WithMessage("La unidad de Entrada debe ser 'm3'.");

        RuleFor(x => x.Proceso.Salidas).NotNull().Must(s => s.Count > 0)
            .WithMessage("Debe especificar al menos una salida.");
        RuleForEach(x => x.Proceso.Salidas).ChildRules(s =>
        {
            s.RuleFor(i => i.ProductoId).GreaterThan(0);
            s.RuleFor(i => i.Cantidad).GreaterThanOrEqualTo(0);
            s.RuleFor(i => i.Unidad).NotEmpty().Must(u => u.Equals("m3", StringComparison.OrdinalIgnoreCase))
                .WithMessage("La unidad de Salida debe ser 'm3'.");
        });

        RuleFor(x => x.Proceso.Salidas)
            .Must(s => s.Select(i => i.ProductoId).Distinct().Count() == s.Count)
            .WithMessage("No se permiten productos duplicados en Salidas.");

        RuleFor(x => x.Proceso.ToleranciaPct)
            .Must(t => t is null || (t >= 0 && t <= 5))
            .WithMessage("ToleranciaPct debe estar entre 0 y 5 (o null para default).");
    }
}
