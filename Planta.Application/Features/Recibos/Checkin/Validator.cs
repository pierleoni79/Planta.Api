// Ruta: /Planta.Application/Features/Recibos/Checkin/Validator.cs | V1.1
using FluentValidation;

namespace Planta.Application.Features.Recibos.Checkin
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ReciboId).NotEmpty();

            // Lat/Lng opcionales; si viene una, debe venir la otra
            RuleFor(x => x.Body.Latitude)
                .InclusiveBetween(-90, 90)
                .When(x => x.Body.Latitude.HasValue);
            RuleFor(x => x.Body.Longitude)
                .InclusiveBetween(-180, 180)
                .When(x => x.Body.Longitude.HasValue);
            RuleFor(x => x.Body)
                .Must(b => (!b.Latitude.HasValue && !b.Longitude.HasValue) ||
                           (b.Latitude.HasValue && b.Longitude.HasValue))
                .WithMessage("Si envías coordenadas, incluye BOTH Latitude y Longitude.");

            RuleFor(x => x.Body.AccuracyMeters)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Body.AccuracyMeters.HasValue);
        }
    }
}
