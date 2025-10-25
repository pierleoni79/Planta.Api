// Ruta: /Planta.Api/Controllers/RecibosController.cs | V2.8
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;                  // StatusCodes
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Planta.Api.Common;                         // ProblemDetails helpers
using Planta.Contracts.Recibos;                  // CheckinRequest + ReciboEstado (enum)
using Planta.Data.Entities;                      // Recibo entity
using IPlantaDbContext = Planta.Application.Abstractions.IPlantaDbContext;

namespace Planta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RecibosController : ControllerBase
{
    private readonly IPlantaDbContext _db;
    private readonly IMemoryCache _cache;

    public RecibosController(IPlantaDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    // ========================
    // ======== GRID ==========
    // ========================

    [HttpGet("grid")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<IActionResult> Grid(
        int page = 1,
        int pageSize = 20,
        int? empresaId = null,
        int? vehiculoId = null,
        int? clienteId = null,
        int? materialId = null,
        DateTimeOffset? desde = null,
        DateTimeOffset? hasta = null,
        string? q = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var etag = await ComputeGridEtagAsync(
            empresaId, vehiculoId, clienteId, materialId, desde, hasta, q, page, pageSize, ct);

        var inm = Request.Headers.IfNoneMatch.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(inm) && string.Equals(inm, etag, StringComparison.Ordinal))
        {
            Response.Headers.ETag = etag;
            Response.Headers.CacheControl = "public, max-age=60";
            return StatusCode(StatusCodes.Status304NotModified);
        }

        var baseQ = ApplyFilters(_db.Query<Recibo>(), empresaId, vehiculoId, clienteId, materialId, desde, hasta, q)
            .AsNoTracking();

        var total = await baseQ.CountAsync(ct);

        var items = await baseQ
            .OrderByDescending(r => r.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.EmpresaId,
                r.Consecutivo,
                r.FechaCreacion,
                Estado = (int)r.Estado,
                r.VehiculoId,
                Placa = r.PlacaSnapshot,
                r.ClienteId,
                r.Cantidad
            })
            .ToListAsync(ct);

        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = "public, max-age=60";

        return Ok(new { page, pageSize, total, items });
    }

    // ==========================
    // ======== DETALLE =========
    // ==========================

    [HttpGet("{id:guid}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken ct = default)
    {
        var r = await _db.Query<Recibo>()
            .Where(x => x.Id == id)
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.Activo,
                x.AlmacenOrigenId,
                x.AutoGenerado,
                x.AutoGeneradoEn,
                x.Cantidad,
                x.ClienteId,
                x.ConductorId,
                x.ConductorNombreSnapshot,
                x.Consecutivo,
                x.DestinoTipo,
                x.EmpresaId,
                Estado = (int)x.Estado,
                x.FechaCreacion,
                x.IdempotencyKey,
                x.MaterialId,
                x.NumeroGenerado,
                x.Observaciones,
                Placa = x.PlacaSnapshot,
                x.ReciboFisicoNumero,
                x.ReciboFisicoNumeroNorm,
                x.UltimaActualizacion,          // DateTimeOffset?
                x.UsuarioCreador,
                x.VehiculoId
            })
            .FirstOrDefaultAsync(ct);

        if (r is null) return NotFound();

        long last = r.UltimaActualizacion.HasValue ? r.UltimaActualizacion.Value.ToUnixTimeSeconds() : 0L;
        var etag = $"W/\"{r.Id:N}-{last}-{r.Estado}\"";

        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = "public, max-age=60";
        return Ok(r);
    }

    // ==========================
    // ========= CHECKIN ========
    // ==========================

    /// <summary>
    /// Check-in en planta: EnTransito_Planta → EnPatioPlanta. Idempotente por IdempotencyKey.
    /// Valida If-Match (ETag) si el cliente lo envía.
    /// </summary>
    [HttpPost("{id:guid}/checkin")]
    public async Task<IActionResult> Checkin(Guid id, [FromBody] CheckinRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var rec = await _db.Query<Recibo>().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rec is null) return NotFound();

        // Concurrency (If-Match) → 412 si no coincide
        var ifMatch = Request.Headers.IfMatch.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(ifMatch))
        {
            var currentEtag = ComputeReciboEtag(rec);
            if (!string.Equals(ifMatch, currentEtag, StringComparison.Ordinal))
                return this.PreconditionFailedProblem("recibos/etag", "Precondición fallida",
                    "If-Match no coincide con la versión actual del recibo.");
        }

        // Idempotencia
        if (!string.IsNullOrWhiteSpace(rec.IdempotencyKey) &&
            string.Equals(rec.IdempotencyKey, req.IdempotencyKey, StringComparison.Ordinal))
        {
            Response.Headers.ETag = ComputeReciboEtag(rec);
            return Ok(new { id = rec.Id, estado = (int)rec.Estado, idempotent = true });
        }
        if (!string.IsNullOrWhiteSpace(rec.IdempotencyKey) &&
            !string.Equals(rec.IdempotencyKey, req.IdempotencyKey, StringComparison.Ordinal))
        {
            return this.ConflictProblem("recibos/idempotencia", "Operación ya ejecutada",
                "El recibo ya fue procesado con otra IdempotencyKey.");
        }

        // Reglas de estado (rec.Estado es byte)
        if (rec.Estado == (byte)ReciboEstado.EnTransito_Planta)
        {
            rec.Estado = (byte)ReciboEstado.EnPatioPlanta; // llegada a planta
        }
        else if (rec.Estado == (byte)ReciboEstado.EnTransito_Cliente)
        {
            return this.ConflictProblem("recibos/destino-cliente", "Flujo no permitido",
                "Check-in solo aplica a recibos con destino Planta (EnTransito_Planta).");
        }
        else
        {
            return this.ConflictProblem("recibos/estado-invalido", "Estado inválido",
                "Solo se permite check-in desde estados EnTransito_*.");
        }

        // Observaciones e idempotencia
        if (!string.IsNullOrWhiteSpace(req.Observaciones))
        {
            rec.Observaciones = string.IsNullOrWhiteSpace(rec.Observaciones)
                ? req.Observaciones
                : $"{rec.Observaciones} | {req.Observaciones}";
        }
        rec.IdempotencyKey = req.IdempotencyKey;
        rec.UltimaActualizacion = DateTimeOffset.UtcNow;

        await ((IPlantaDbContext)_db).SaveChangesAsync(ct);

        Response.Headers.ETag = ComputeReciboEtag(rec);
        return Ok(new { id = rec.Id, estado = (int)rec.Estado, updated = true });
    }

    // ==========================
    // ====== HELPERS PRIV ======
    // ==========================

    private static IQueryable<Recibo> ApplyFilters(
        IQueryable<Recibo> q,
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
            q = q.Where(r =>
                (r.PlacaSnapshot ?? string.Empty).Contains(qtext) ||
                (r.Observaciones ?? string.Empty).Contains(qtext));
        return q;
    }

    private static string ComputeReciboEtag(Recibo r)
    {
        long last = r.UltimaActualizacion.HasValue
            ? r.UltimaActualizacion.Value.ToUnixTimeSeconds()
            : 0L;
        int estado = (int)r.Estado; // r.Estado es byte; lo exponemos como int
        return $"W/\"{r.Id:N}-{last}-{estado}\"";
    }

    private async Task<string> ComputeGridEtagAsync(
        int? empresaId, int? vehiculoId, int? clienteId, int? materialId,
        DateTimeOffset? desde, DateTimeOffset? hasta, string? q,
        int page, int pageSize, CancellationToken ct)
    {
        var key = $"recibos-grid:v1:empresa={empresaId};veh={vehiculoId};cli={clienteId};mat={materialId};" +
                  $"desde={desde?.ToUnixTimeSeconds()};hasta={hasta?.ToUnixTimeSeconds()};q={q};" +
                  $"page={page};size={pageSize}";

        if (_cache.TryGetValue<string>(key, out var etagCached) && etagCached is not null)
            return etagCached;

        var baseQ = ApplyFilters(_db.Query<Recibo>(), empresaId, vehiculoId, clienteId, materialId, desde, hasta, q);

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

        _cache.Set(key, etag, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
        });

        return etag;
    }
}
