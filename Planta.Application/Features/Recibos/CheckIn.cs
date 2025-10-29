// Ruta: /Planta.Application/Features/Recibos/CheckIn.cs | V2.0
#nullable enable
using MediatR;
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;
using Planta.Application.Common;
using Planta.Contracts.Recibos;
using Planta.Data.Entities;
using Planta.Domain.Recibos;

namespace Planta.Application.Features.Recibos;

public sealed record CheckInCommand(Guid Id, string? IfMatch, string? IdempotencyKey, string? Comentario, string? Gps)
    : IRequest<(ReciboDetailDto Dto, string ETag, bool Idempotent)>;

public sealed class CheckInHandler : IRequestHandler<CheckInCommand, (ReciboDetailDto Dto, string ETag, bool Idempotent)>
{
    private const string Scope = "checkin";
    private readonly IPlantaDbContext _db;
    public CheckInHandler(IPlantaDbContext db) => _db = db;

    public async Task<(ReciboDetailDto Dto, string ETag, bool Idempotent)> Handle(CheckInCommand c, CancellationToken ct)
    {
        var rec = await _db.Query<Recibo>().FirstOrDefaultAsync(r => r.Id == c.Id, ct)
                  ?? throw new KeyNotFoundException("Recibo no encontrado.");

        if (!string.IsNullOrWhiteSpace(rec.IdempotencyKey) && rec.IdempotencyKey.StartsWith(Scope + ":", StringComparison.Ordinal))
        {
            var prevKey = rec.IdempotencyKey.Split(':').LastOrDefault();
            if (!string.IsNullOrWhiteSpace(c.IdempotencyKey) && string.Equals($"{Scope}:{c.IdempotencyKey}", rec.IdempotencyKey, StringComparison.Ordinal))
            {
                var etExisting = EtagHelper.ForRecibo(rec.Consecutivo, rec.UltimaActualizacion, rec.FechaCreacion, rec.Estado);
                return (Project(rec), etExisting, true);
            }
            if (!string.IsNullOrWhiteSpace(c.IdempotencyKey) && !string.Equals(prevKey, c.IdempotencyKey, StringComparison.Ordinal))
                throw new ConflictException("recibos/idempotencia", "Operación ya ejecutada. Use la misma IdempotencyKey.");
        }

        var currentEtag = EtagHelper.ForRecibo(rec.Consecutivo, rec.UltimaActualizacion, rec.FechaCreacion, rec.Estado);
        if (!string.IsNullOrWhiteSpace(c.IfMatch) && !string.Equals(c.IfMatch, currentEtag, StringComparison.Ordinal))
            throw new PreconditionFailedException("recibos/etag", "If-Match no coincide (versión desactualizada).");

        if (rec.Estado != (int)ReciboEstado.EnTransito_Planta)
            throw new ConflictException("recibos/estado-invalido", "Solo se permite check-in desde 'EnTransito_Planta'.");

        rec.Estado = (int)ReciboEstado.EnPatioPlanta;
        if (!string.IsNullOrWhiteSpace(c.Comentario))
            rec.Observaciones = string.IsNullOrWhiteSpace(rec.Observaciones) ? c.Comentario : $"{rec.Observaciones} | {c.Comentario}";

        if (!string.IsNullOrWhiteSpace(c.IdempotencyKey))
            rec.IdempotencyKey = $"{Scope}:{c.IdempotencyKey}";
        rec.UltimaActualizacion = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        var etag = EtagHelper.ForRecibo(rec.Consecutivo, rec.UltimaActualizacion, rec.FechaCreacion, rec.Estado);
        return (Project(rec), etag, false);
    }

    private static ReciboDetailDto Project(Recibo r) => new()
    {
        Id = r.Id,
        EmpresaId = r.EmpresaId,
        Consecutivo = r.Consecutivo,
        FechaCreacion = r.FechaCreacion,
        Estado = (ReciboEstado)r.Estado,
        VehiculoId = r.VehiculoId,
        Placa = r.PlacaSnapshot,
        ClienteId = r.ClienteId,
        Cantidad = r.Cantidad,
        ConductorId = r.ConductorId,
        ConductorNombreSnapshot = r.ConductorNombreSnapshot,
        Observaciones = r.Observaciones
    };
}
