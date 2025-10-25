// Ruta: /Planta.Api/Controllers/_Helpers/RecibosGridCache.cs | V1.2
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Planta.Data.Context;
using Planta.Data.Entities;
using IPlantaDbContext = Planta.Application.Abstractions.IPlantaDbContext;

namespace Planta.Api.Controllers._Helpers;

internal static class RecibosGridCache
{
    private static IQueryable<Recibo> ApplyFilters(IQueryable<Recibo> q,
        int? empresaId, int? vehiculoId, int? clienteId, int? materialId,
        DateTimeOffset? desde, DateTimeOffset? hasta, string? qtext)
    {
        if (empresaId.HasValue) q = q.Where(r => r.EmpresaId == empresaId.Value);
        if (vehiculoId.HasValue) q = q.Where(r => r.VehiculoId == vehiculoId.Value);
        if (clienteId.HasValue) q = q.Where(r => r.ClienteId == clienteId.Value);
        if (materialId.HasValue) q = q.Where(r => r.MaterialId == materialId.Value);
        if (desde.HasValue) q = q.Where(r => r.FechaCreacion >= desde.Value);
        if (hasta.HasValue) q = q.Where(r => r.FechaCreacion <= hasta.Value);
        if (!string.IsNullOrWhiteSpace(qtext))
            q = q.Where(r => (r.PlacaSnapshot ?? string.Empty).Contains(qtext)
                          || (r.Observaciones ?? string.Empty).Contains(qtext));
        return q;
    }

    public static async Task<string> ComputeGridEtagAsync(HttpContext http,
        IPlantaDbContext db, IMemoryCache cache,
        int? empresaId, int? vehiculoId, int? clienteId, int? materialId,
        DateTimeOffset? desde, DateTimeOffset? hasta, string? q,
        int page, int pageSize, CancellationToken ct)
    {
        var key = $"recibos-grid:v1:empresa={empresaId};veh={vehiculoId};cli={clienteId};mat={materialId};" +
                  $"desde={desde?.ToUnixTimeSeconds()};hasta={hasta?.ToUnixTimeSeconds()};q={q};" +
                  $"page={page};size={pageSize}";

        // ✅ Null-safety: usa genérico y valida antes de retornar (evita CS8600/CS8603)
        if (cache.TryGetValue<string>(key, out var etagCached))
        {
            if (!string.IsNullOrEmpty(etagCached))
                return etagCached;
        }

        var baseQ = ApplyFilters(db.Query<Recibo>(), empresaId, vehiculoId, clienteId, materialId, desde, hasta, q);

        var sig = await baseQ
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                MaxCon = g.Max(x => (int?)x.Consecutivo) ?? 0,
                MaxUpd = g.Max(x => (DateTimeOffset?)x.UltimaActualizacion) ?? DateTimeOffset.MinValue
            })
            .FirstOrDefaultAsync(ct);

        var etag = $"W/\"{sig?.Count ?? 0}-{sig?.MaxCon ?? 0}-{(sig?.MaxUpd ?? DateTimeOffset.MinValue).ToUnixTimeSeconds()}\"";

        cache.Set(key, etag, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
        });

        return etag;
    }
}
