// Ruta: /Planta.Application/Validators/Common/PagedRequestValidator.cs | V1.0
#nullable enable
using FluentValidation;
using Planta.Contracts.Common;

namespace Planta.Application.Validators.Common;

public sealed class PagedRequestValidator : AbstractValidator<PagedRequest>
{
    public PagedRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(PagedRequest.MinPageSize, PagedRequest.MaxPageSize);
    }
}
