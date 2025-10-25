// Ruta: /Planta.Application/Features/Recibos/Checkin/Handler.cs | V1.1
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;
using Planta.Domain.Operaciones;

namespace Planta.Application.Features.Recibos.Checkin
{
    public sealed class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IPlantaDbContext _db;
        public Handler(IPlantaDbContext db) => _db = db;

        // TODO: reemplazar por los códigos reales de tu enum tinyint
        private const byte EnTransito_Planta = 20;
        private const byte EnPatioPlanta = 30;

        public async Task<Unit> Handle(Command request, CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;
            var body = request.Body;

            // 1) Cargar recibo
            var recibo = await _db.Set<Recibo>()
                .FirstOrDefaultAsync(r => r.Id == request.ReciboId, ct)
                ?? throw new InvalidOperationException("Recibo no existe.");

            // 2) Idempotencia suave
            if (recibo.Estado == EnPatioPlanta)
                return Unit.Value;

            // 3) Validar transición
            if (recibo.Estado != EnTransito_Planta)
                throw new InvalidOperationException("Transición no permitida: el recibo no está en tránsito a planta.");

            // 4) Armar GPS (opcional)
            var parts = new List<string>();
            if (body.Latitude.HasValue && body.Longitude.HasValue)
            {
                parts.Add(string.Create(CultureInfo.InvariantCulture,
                    $"{body.Latitude.Value:0.#####},{body.Longitude.Value:0.#####}"));
            }
            if (body.AccuracyMeters is not null) parts.Add(string.Create(CultureInfo.InvariantCulture, $"acc={body.AccuracyMeters.Value:0.#}"));
            parts.Add($"src={(string.IsNullOrWhiteSpace(body.Source) ? "manual" : body.Source!.Trim())}");
            if (body.DeviceTimeUtc is not null) parts.Add($"dev={body.DeviceTimeUtc:O}");
            var gps = string.Join(';', parts);   // p.ej., "src=manual" si no hubo coords

            // 5) Cambiar estado + bitácora
            recibo.Estado = EnPatioPlanta;
            recibo.UltimaActualizacion = now;

            _db.Set<ReciboEstadoLog>().Add(new ReciboEstadoLog
            {
                ReciboId = recibo.Id,
                Estado = EnPatioPlanta,
                Nota = body.Notes,
                GPS = gps,
                Cuando = now   // (la BD también setea default)
            });

            await _db.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
