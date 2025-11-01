// Ruta: /Planta.Application/Tarifas/Queries/ObtenerTarifaVigente/ObtenerTarifaVigenteValidator.cs | V1.1 (fix: alias DestinoTipo para evitar ambigüedad)
#nullable enable
using FluentValidation;
// Alias para forzar el enum de Contracts y evitar choque con Planta.Domain.Enums
using DestinoTipoApi = Planta.Contracts.Enums.DestinoTipo;

namespace Planta.Application.Tarifas.Queries.ObtenerTarifaVigente;

public sealed class ObtenerTarifaVigenteValidator : AbstractValidator<ObtenerTarifaVigenteQuery>
{
    public ObtenerTarifaVigenteValidator()
    {
        RuleFor(x => x.ClaseVehiculoId).GreaterThan(0);
        RuleFor(x => x.MaterialId).GreaterThan(0);

        When(x => x.Destino == DestinoTipoApi.ClienteDirecto, () =>
        {
            RuleFor(x => x.ClienteId)
                .NotNull().WithMessage("ClienteId es requerido cuando el destino es ClienteDirecto.")
                .GreaterThan(0);
        });

        When(x => x.Destino == DestinoTipoApi.Planta, () =>
        {
            RuleFor(x => x.PlantaId)
                .NotNull().WithMessage("PlantaId es requerido cuando el destino es Planta.")
                .GreaterThan(0);
        });
    }
}
