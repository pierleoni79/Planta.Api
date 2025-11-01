// Ruta: /Planta.Application/Recibos/Validators/ReciboValidators.cs | V1.1
#nullable enable
using Planta.Contracts.Recibos.Queries;
using Planta.Contracts.Recibos.Requests;

namespace Planta.Application.Recibos.Validators;

public sealed class ReciboCreateRequestValidator : AbstractValidator<ReciboCreateRequest>
{
    public ReciboCreateRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.EmpresaId).GreaterThan(0);
        RuleFor(x => x.Cantidad).GreaterThan(0m);
    }
}

public sealed class ReciboUpdateRequestValidator : AbstractValidator<ReciboUpdateRequest>
{
    public ReciboUpdateRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NumeroGenerado).MaximumLength(64).When(x => x.NumeroGenerado is not null);
        RuleFor(x => x.ReciboFisicoNumero).MaximumLength(64).When(x => x.ReciboFisicoNumero is not null);
    }
}

public sealed class CambiarEstadoRequestValidator : AbstractValidator<CambiarEstadoRequest>
{
    public CambiarEstadoRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NuevoEstado).IsInEnum();
    }
}

public sealed class VincularTransporteRequestValidator : AbstractValidator<VincularTransporteRequest>
{
    public VincularTransporteRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PlacaSnapshot).MaximumLength(32).When(x => x.PlacaSnapshot is not null);
        RuleFor(x => x.ConductorNombreSnapshot).MaximumLength(128).When(x => x.ConductorNombreSnapshot is not null);
    }
}

public sealed class VincularOrigenRequestValidator : AbstractValidator<VincularOrigenRequest>
{
    public VincularOrigenRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PlantaId).GreaterThan(0).When(x => x.PlantaId is not null);
        RuleFor(x => x.AlmacenOrigenId).GreaterThan(0).When(x => x.AlmacenOrigenId is not null);
        RuleFor(x => x.ClienteId).GreaterThan(0).When(x => x.ClienteId is not null);
    }
}

public sealed class RegistrarMaterialRequestValidator : AbstractValidator<RegistrarMaterialRequest>
{
    public RegistrarMaterialRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.MaterialId).GreaterThan(0);
        RuleFor(x => x.Cantidad).GreaterThan(0m);
    }
}

public sealed class ReciboListQueryValidator : AbstractValidator<ReciboListQuery>
{
    public ReciboListQueryValidator()
    {
        RuleFor(x => x.Paging.Page).GreaterThan(0);
        RuleFor(x => x.Paging.PageSize).InclusiveBetween(1, 200);
    }
}
