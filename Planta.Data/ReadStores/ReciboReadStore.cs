// Ruta: /Planta.Data/ReadStores/ReciboReadStore.cs | V1.5 (DTOs usan DateTime)
#nullable enable
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Planta.Contracts.Common;
using Planta.Contracts.Enums;
using Planta.Contracts.Recibos;
using Planta.Domain.Recibos;

namespace Planta.Data.ReadStores;

public sealed class ReciboReadStore : IReciboReadStore
{
    private readonly PlantaDbContext _db;
    public ReciboReadStore(PlantaDbContext db) => _db = db;

    private static DateTimeOffset ToUtcOffset(DateTime dt)
    {
        if (dt.Kind == DateTimeKind.Unspecified)
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        return new DateTimeOffset(dt.ToUniversalTime(), TimeSpan.Zero);
    }

    public async Task<PagedResult<ReciboListItemDto>> ListAsync(ReciboListQuery q, CancellationToken ct)
    {
        var f = q.Filter;
        IQueryable<Recibo> query = _db.Recibos.AsNoTracking();

        if (f.EmpresaId is int emp) query = query.Where(x => x.EmpresaId == emp);
        if (f.Estado is ReciboEstado est) query = query.Where(x => x.Estado == (int)est);
        if (f.DestinoTipo is DestinoTipo dst) query = query.Where(x => x.DestinoTipo == (int)dst);
        if (f.VehiculoId is int veh) query = query.Where(x => x.VehiculoId == veh);
        if (f.ConductorId is int con) query = query.Where(x => x.ConductorId == con);
        if (f.ClienteId is int cli) query = query.Where(x => x.ClienteId == cli);
        if (f.MaterialId is int mat) query = query.Where(x => x.MaterialId == mat);
        if (f.FechaDesdeUtc is DateTime fd) query = query.Where(x => x.FechaCreacionUtc >= ToUtcOffset(fd));
        if (f.FechaHastaUtc is DateTime fh) query = query.Where(x => x.FechaCreacionUtc <= ToUtcOffset(fh));

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var s = f.Search.Trim();
            query = query.Where(x =>
                   (x.NumeroGenerado ?? "").Contains(s)
                || (x.PlacaSnapshot ?? "").Contains(s)
                || (x.ConductorNombreSnapshot ?? "").Contains(s)
                || (x.Observaciones ?? "").Contains(s)
                || (x.ReciboFisicoNumero ?? "").Contains(s));
        }

        // Orden
        query = ApplySort(query, q.Paging?.Sort);

        var total = await query.CountAsync(ct);
        var page = Math.Max(q.Paging?.Page ?? 1, 1);
        var size = Math.Clamp(q.Paging?.PageSize ?? 20, 1, 200);

        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Select(x => new
            {
                x.Id,
                x.EmpresaId,
                x.Estado,
                x.DestinoTipo,
                x.Cantidad,
                x.PlacaSnapshot,
                x.ConductorNombreSnapshot,
                x.NumeroGenerado,
                x.FechaCreacionUtc,       // DateTimeOffset
                x.UltimaActualizacionUtc  // DateTimeOffset?
            })
            .ToListAsync(ct);

        var dtoItems = items.Select(x =>
        {
            var baseTimeDto = x.UltimaActualizacionUtc.HasValue
                ? x.UltimaActualizacionUtc.Value.UtcDateTime
                : x.FechaCreacionUtc.UtcDateTime;

            var verTicks = baseTimeDto.Ticks;
            var etag = $"W/\"{x.Id:N}-{verTicks}\"";

            return new ReciboListItemDto(
                x.Id,
                x.EmpresaId,
                (ReciboEstado)x.Estado,
                (DestinoTipo)x.DestinoTipo,
                x.Cantidad,
                x.PlacaSnapshot,
                x.ConductorNombreSnapshot,
                x.NumeroGenerado,
                x.FechaCreacionUtc.UtcDateTime, // <-- DTO espera DateTime
                etag
            );
        }).ToList();

        return new PagedResult<ReciboListItemDto>(dtoItems, total, page, size);
    }

    public async Task<ReciboDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var x = await _db.Recibos.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.Id,
                r.EmpresaId,
                r.PlantaId,
                r.AlmacenOrigenId,
                r.ClienteId,
                r.VehiculoId,
                r.ConductorId,
                r.MaterialId,
                r.Estado,
                r.DestinoTipo,
                r.Cantidad,
                r.Observaciones,
                r.PlacaSnapshot,
                r.ConductorNombreSnapshot,
                r.ReciboFisicoNumero,
                r.NumeroGenerado,
                r.IdempotencyKey,
                r.FechaCreacionUtc,       // DateTimeOffset
                r.UltimaActualizacionUtc  // DateTimeOffset?
            })
            .FirstOrDefaultAsync(ct);

        if (x is null) return null;

        var baseTimeDto = x.UltimaActualizacionUtc.HasValue
            ? x.UltimaActualizacionUtc.Value.UtcDateTime
            : x.FechaCreacionUtc.UtcDateTime;

        var etag = $"W/\"{x.Id:N}-{baseTimeDto.Ticks}\"";

        return new ReciboDto(
            x.Id,
            x.EmpresaId,
            x.PlantaId,
            x.AlmacenOrigenId,
            x.ClienteId,
            x.VehiculoId,
            x.ConductorId,
            x.MaterialId,
            (ReciboEstado)x.Estado,
            (DestinoTipo)x.DestinoTipo,
            x.Cantidad,
            x.Observaciones,
            x.PlacaSnapshot,
            x.ConductorNombreSnapshot,
            x.ReciboFisicoNumero,
            x.NumeroGenerado,
            x.IdempotencyKey,
            x.FechaCreacionUtc.UtcDateTime,                          // <-- DTO espera DateTime
            x.UltimaActualizacionUtc?.UtcDateTime ??                 // <-- nullable → DateTime
                x.FechaCreacionUtc.UtcDateTime,
            etag
        );
    }

    private static IQueryable<Recibo> ApplySort(IQueryable<Recibo> q, IReadOnlyList<SortSpec>? sort)
    {
        if (sort is null || sort.Count == 0)
            return q.OrderByDescending(x => x.FechaCreacionUtc).ThenBy(x => x.Id);

        IOrderedQueryable<Recibo>? ordered = null;
        foreach (var s in sort)
        {
            Expression<Func<Recibo, object>> key = s.Field switch
            {
                nameof(Recibo.EmpresaId) => x => x.EmpresaId,
                nameof(Recibo.Estado) => x => x.Estado,
                nameof(Recibo.DestinoTipo) => x => x.DestinoTipo,
                nameof(Recibo.Cantidad) => x => x.Cantidad,
                nameof(Recibo.NumeroGenerado) => x => x.NumeroGenerado!,
                nameof(Recibo.FechaCreacionUtc) => x => x.FechaCreacionUtc,
                _ => x => x.FechaCreacionUtc
            };

            ordered = ordered is null
                ? (s.Desc ? q.OrderByDescending(key) : q.OrderBy(key))
                : (s.Desc ? ordered.ThenByDescending(key) : ordered.ThenBy(key));
        }
        return ordered ?? q;
    }
}
