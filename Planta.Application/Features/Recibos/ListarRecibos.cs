// Ruta: /Planta.Application/Features/Recibos/ListarRecibos.cs | V2.0
#nullable enable
using MediatR;
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;
using Planta.Application.Common;
using Planta.Contracts.Recibos;
using Planta.Data.Entities;
using Planta.Domain.Recibos;

namespace Planta.Application.Features.Recibos;

public sealed record ListarRecibosQuery(
    int? EmpresaId, int? VehiculoId, int? ClienteId, int? MaterialId,
    DateTimeOffset? Desde, DateTimeOffset? Hasta, string? Search,
    string? IfNoneMatch
) : IRequest<ListarRecibosResult>;

public sealed record ListarRecibosResult(IReadOnlyList<ReciboListItemDto> Items, string ETag, bool NotModified);

public sealed class ListarRecibosHandler : IRequestHandler<ListarRecibosQuery, ListarRecibosResult>
{
    private readonly IPlantaDbContext _db;
    public ListarRecibosHandler(IPlantaDbContext db) => _db = db;

    public async Task<ListarRecibosResult> Handle(ListarRecibosQuery q, CancellationToken ct)
    {
        IQueryable<Recibo> baseQ = _db.Query<Recibo>();

        if (q.EmpresaId is not null) baseQ = baseQ.Where(r => r.EmpresaId == q.EmpresaId);
        if (q.VehiculoId is not null) baseQ = baseQ.Where(r => r.VehiculoId == q.VehiculoId);
        if (q.ClienteId is not null) baseQ = baseQ.Where(r => r.ClienteId == q.ClienteId);
        if (q.MaterialId is not null) baseQ = baseQ.Where(r => r.MaterialId == q.MaterialId);
        if (q.Desde is not null) baseQ = baseQ.Where(r => r.FechaCreacion >= q.Desde);
        if (q.Hasta is not null) baseQ = baseQ.Where(r => r.FechaCreacion <= q.Hasta);
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = $"%{q.Search.Trim()}%";
            baseQ = baseQ.Where(r =>
                EF.Functions.Like(r.PlacaSnapshot ?? string.Empty, s) ||
                EF.Functions.Like(r.Observaciones ?? string.Empty, s));
        }

        // Firma para ETag (igual a tu Grid-ETag)
        var sig = await baseQ
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                MaxCon = g.Max(x => (int?)x.Consecutivo) ?? 0,
                MaxUpd = g.Max(x => (DateTimeOffset?)x.UltimaActualizacion) ?? DateTimeOffset.MinValue
            })
            .FirstOrDefaultAsync(ct);

        var listETag = $"W/\"{sig?.Count ?? 0}-{sig?.MaxCon ?? 0}-{(sig?.MaxUpd ?? DateTimeOffset.MinValue).ToUnixTimeSeconds()}\"";
        if (!string.IsNullOrWhiteSpace(q.IfNoneMatch) && string.Equals(q.IfNoneMatch, listETag, StringComparison.Ordinal))
            return new ListarRecibosResult(Array.Empty<ReciboListItemDto>(), listETag, true);

        var items = await baseQ
            .AsNoTracking()
            .OrderByDescending(r => r.FechaCreacion)
            .Select(r => new ReciboListItemDto
            {
                Id = r.Id,
                EmpresaId = r.EmpresaId,
                Consecutivo = r.Consecutivo,
                FechaCreacion = r.FechaCreacion,
                Estado = (ReciboEstado)r.Estado,
                VehiculoId = r.VehiculoId,
                Placa = r.PlacaSnapshot,
                ClienteId = r.ClienteId,
                Cantidad = r.Cantidad
            })
            .ToListAsync(ct);

        return new ListarRecibosResult(items, listETag, false);
    }
}
