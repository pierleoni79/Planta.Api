// Ruta: /Planta.Application/Features/Recibos/DescargaInicio.cs | V1.0
#nullable enable
using MediatR;
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;
using Planta.Application.Common;
using Planta.Contracts.Recibos;
using Planta.Data.Entities;
using Planta.Domain.Recibos;

namespace Planta.Application.Features.Recibos;

public sealed record DescargaInicioCommand(Guid Id, string? IfMatch, string? IdempotencyKey, string? Observaciones)
    : IRequest<(ReciboDetailDto Dto, string ETag, bool Idempotent)>;

public sealed class DescargaInicioHandler : IRequestHandler<DescargaInicioCommand, (ReciboDetailDto Dto, string ETag, bool Idempotent)>
{
    private const string Scope = "descarga-inicio";
    private readonly IPlantaDbContext _db;
    public DescargaInicioHandler(IPlantaDbContext db) => _db = db;

    public async Task<(ReciboDetailDto Dto, string ETag, bool Idempotent)> Handle(DescargaInicioCommand c, CancellationToken ct)
    {
        var rec = await _db.Query<Recibo>().FirstOrDefaultAsync(r => r.Id == c.Id, ct)
                  ?? throw new KeyNotFoundException("Recibo no encontrado.");

        if (!string.IsNullOrWhiteSpace(rec.IdempotencyKey) && rec.IdempotencyKey.StartsWith(Scope + ":", StringComparison.Ordinal))
        {
            if (!string.IsNullOrWhiteSpace(c.IdempotencyKey) && string.Equals($"{Scope}:{c.IdempotencyKey}", rec.IdempotencyKey, StringComparison.Ordinal))
                return (Project(rec), EtagHelper.ForRecibo(rec.Consecutivo, rec.UltimaActualizacion, rec.FechaCreacion, rec.Estado), true);
            if (!string.IsNullOrWhiteSpace(c.IdempotencyKey))
                throw new ConflictException("recibos/idempotencia", "Operación ya ejecutada. Use la misma IdempotencyKey.");
        }

        var currentEtag = EtagHelper.ForRecibo(rec.Consecutivo, rec.UltimaActualizacion, rec.FechaCreacion, rec.Estado);
        if (!string.IsNullOrWhiteSpace(c.IfMatch) && !string.Equals(c.IfMatch, currentEtag, StringComparison.Ordinal))
            throw new PreconditionFailedException("recibos/etag", "If-Match no coincide.");

        if (rec.Estado != (int)ReciboEstado.EnPatioPlanta)
            throw new ConflictException("recibos/estado-invalido", "Solo se permite iniciar descarga desde 'EnPatioPlanta'.");

        rec.Estado = (int)ReciboEstado.Descargando;
        if (!string.IsNullOrWhiteSpace(c.Observaciones))
            rec.Observaciones = string.IsNullOrWhiteSpace(rec.Observaciones) ? c.Observaciones : $"{rec.Observaciones} | {c.Observaciones}";

        if (!string.IsNullOrWhiteSpace(c.IdempotencyKey)) rec.IdempotencyKey = $"{Scope}:{c.IdempotencyKey}";
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
