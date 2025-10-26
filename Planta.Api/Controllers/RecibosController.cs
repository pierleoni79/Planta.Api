// Ruta: /Planta.Api/Controllers/RecibosController.cs | V2.11
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Planta.Api.Common;
using Planta.Contracts.Recibos;
using Planta.Data.Entities;                      // Recibo, Proceso, ProcesoDet
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    // -------- GRID --------
    [HttpGet("grid")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<IActionResult> Grid(
        int page = 1, int pageSize = 20,
        int? empresaId = null, int? vehiculoId = null, int? clienteId = null, int? materialId = null,
        DateTimeOffset? desde = null, DateTimeOffset? hasta = null, string? q = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var etag = await ComputeGridEtagAsync(empresaId, vehiculoId, clienteId, materialId, desde, hasta, q, page, pageSize, ct);

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

    // -------- DETALLE --------
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
                x.UltimaActualizacion,
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

    // ========= CHECKIN =========
    [HttpPost("{id:guid}/checkin")]
    public async Task<IActionResult> Checkin(Guid id, [FromBody] CheckinRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var rec = await _db.Query<Recibo>().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rec is null) return NotFound();

        // 1) Idempotencia (scope)
        const string scope = "checkin";
        if (IsSameScopeIdem(rec.IdempotencyKey, scope, req.IdempotencyKey))
        {
            Response.Headers.ETag = ComputeReciboEtag(rec);
            return Ok(new { id = rec.Id, estado = (int)rec.Estado, idempotent = true });
        }
        if (HasDifferentIdemSameScope(rec.IdempotencyKey, scope, req.IdempotencyKey))
            return this.ConflictProblem("recibos/idempotencia", "Operación ya ejecutada", "Use la misma IdempotencyKey.");

        // 2) If-Match (precondición)
        var ifMatch = Request.Headers.IfMatch.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(ifMatch) && !string.Equals(ifMatch, ComputeReciboEtag(rec), StringComparison.Ordinal))
            return this.PreconditionFailedProblem("recibos/etag", "Precondición fallida", "If-Match no coincide.");

        // 3) Reglas de estado
        if (rec.Estado == (byte)ReciboEstado.EnTransito_Planta)
        {
            rec.Estado = (byte)ReciboEstado.EnPatioPlanta;
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

        // 4) Lógica/Persistencia
        if (!string.IsNullOrWhiteSpace(req.Observaciones))
            rec.Observaciones = string.IsNullOrWhiteSpace(rec.Observaciones)
                ? req.Observaciones
                : $"{rec.Observaciones} | {req.Observaciones}";

        SetScopedIdem(rec, scope, req.IdempotencyKey);
        rec.UltimaActualizacion = DateTimeOffset.UtcNow;

        try
        {
            await ((IPlantaDbContext)_db).SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            return this.ConflictProblem("db/update", "Error al guardar", ex.InnerException?.Message ?? ex.Message);
        }

        Response.Headers.ETag = ComputeReciboEtag(rec);
        return Ok(new { id = rec.Id, estado = (int)rec.Estado, updated = true });
    }


    // ===== INICIO DE DESCARGA → estado 30 =====
    [HttpPost("{id:guid}/descarga/inicio")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> DescargaInicio(Guid id, [FromBody] DescargaInicioRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var rec = await _db.Query<Recibo>().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rec is null) return NotFound();

        // 1) Idempotencia (scope)
        const string scope = "descarga-inicio";
        if (IsSameScopeIdem(rec.IdempotencyKey, scope, req.IdempotencyKey))
        {
            Response.Headers.ETag = ComputeReciboEtag(rec);
            return Ok(new { id = rec.Id, estado = (int)rec.Estado, idempotent = true });
        }
        if (HasDifferentIdemSameScope(rec.IdempotencyKey, scope, req.IdempotencyKey))
            return this.ConflictProblem("recibos/idempotencia", "Operación ya ejecutada", "Use la misma IdempotencyKey.");

        // 2) If-Match
        var ifMatch = Request.Headers.IfMatch.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(ifMatch) && !string.Equals(ifMatch, ComputeReciboEtag(rec), StringComparison.Ordinal))
            return this.PreconditionFailedProblem("recibos/etag", "Precondición fallida", "If-Match no coincide.");

        // 3) Estado
        if (rec.Estado != (byte)ReciboEstado.EnPatioPlanta)
            return this.ConflictProblem("recibos/estado-invalido", "Estado inválido",
                "Solo se permite iniciar descarga desde 'EnPatioPlanta'.");

        // 4) Lógica/Persistencia
        rec.Estado = (byte)ReciboEstado.Descargando;
        if (!string.IsNullOrWhiteSpace(req.Observaciones))
            rec.Observaciones = string.IsNullOrWhiteSpace(rec.Observaciones)
                ? req.Observaciones
                : $"{rec.Observaciones} | {req.Observaciones}";

        SetScopedIdem(rec, scope, req.IdempotencyKey);
        rec.UltimaActualizacion = DateTimeOffset.UtcNow;

        try
        {
            await ((IPlantaDbContext)_db).SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            return this.ConflictProblem("db/update", "Error al guardar", ex.InnerException?.Message ?? ex.Message);
        }

        Response.Headers.ETag = ComputeReciboEtag(rec);
        return Ok(new { id = rec.Id, estado = (int)rec.Estado, updated = true });
    }


    // ===== FIN DE DESCARGA → Proceso + estado 40 =====
    [HttpPost("{id:guid}/descarga/fin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> DescargaFin(Guid id, [FromBody] DescargaFinRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var rec = await _db.Query<Recibo>().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rec is null) return NotFound();

        // 1) Idempotencia (scope)
        const string scope = "descarga-fin";
        if (IsSameScopeIdem(rec.IdempotencyKey, scope, req.IdempotencyKey))
        {
            Response.Headers.ETag = ComputeReciboEtag(rec);
            return Ok(new { id = rec.Id, estado = (int)rec.Estado, idempotent = true });
        }
        if (HasDifferentIdemSameScope(rec.IdempotencyKey, scope, req.IdempotencyKey))
            return this.ConflictProblem("recibos/idempotencia", "Operación ya ejecutada", "Use la misma IdempotencyKey.");

        // 2) If-Match
        var ifMatch = Request.Headers.IfMatch.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(ifMatch) && !string.Equals(ifMatch, ComputeReciboEtag(rec), StringComparison.Ordinal))
            return this.PreconditionFailedProblem("recibos/etag", "Precondición fallida", "If-Match no coincide.");

        // 3) Estado (20 o 30)
        if (rec.Estado != (byte)ReciboEstado.EnPatioPlanta &&
            rec.Estado != (byte)ReciboEstado.Descargando)
            return this.ConflictProblem("recibos/estado-invalido", "Estado inválido",
                "El fin de descarga solo aplica cuando el recibo está EnPatioPlanta o Descargando.");

        // 4) Validaciones de proceso (control de masa)
        if (req.Proceso?.Salidas is null || req.Proceso.Salidas.Count == 0)
            return this.ValidationProblem("recibos/proceso", "Debe incluir al menos una salida.");

        var entrada = req.Proceso.Entrada.Cantidad;
        var totalSalidas = req.Proceso.Salidas.Sum(s => s.Cantidad);
        if (entrada <= 0 || totalSalidas <= 0)
            return this.ValidationProblem("recibos/proceso", "Entrada y salidas deben ser > 0.");

        var delta = Math.Abs(entrada - totalSalidas);
        var tolPct = req.Proceso.ToleranciaPct ?? 1.0m; // default 1%
        var maxDelta = (entrada * tolPct) / 100m;

        if (delta > maxDelta)
            return this.ConflictProblem("recibos/proceso/descuadre", "Control de masa falló",
                $"Δ={delta} m3 fuera de tolerancia ({tolPct}%).");

        // 5) Persistir Proceso + Detalles y actualizar Recibo
        var proc = new Proceso
        {
            ReciboId = rec.Id,
            RecetaId = req.Proceso.RecetaId,
            EntradaM3 = entrada,
            Observacion = string.IsNullOrWhiteSpace(req.Observaciones) ? null : req.Observaciones,
            CreadoEn = DateTimeOffset.UtcNow
        };

        try
        {
            await _db.AddAsync(proc, ct);
            await ((IPlantaDbContext)_db).SaveChangesAsync(ct); // genera Id

            foreach (var s in req.Proceso.Salidas)
            {
                var det = new ProcesoDet
                {
                    ProcesoId = proc.Id,
                    ProductoId = s.ProductoId,
                    CantidadM3 = s.Cantidad
                };
                await _db.AddAsync(det, ct);
            }

            rec.Estado = (byte)ReciboEstado.Procesado;
            if (!string.IsNullOrWhiteSpace(req.Observaciones))
                rec.Observaciones = string.IsNullOrWhiteSpace(rec.Observaciones)
                    ? req.Observaciones
                    : $"{rec.Observaciones} | {req.Observaciones}";

            SetScopedIdem(rec, scope, req.IdempotencyKey);
            rec.UltimaActualizacion = DateTimeOffset.UtcNow;

            await ((IPlantaDbContext)_db).SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            return this.ConflictProblem("db/update", "Error al guardar", ex.InnerException?.Message ?? ex.Message);
        }

        Response.Headers.ETag = ComputeReciboEtag(rec);
        return Ok(new { id = rec.Id, estado = (int)rec.Estado, procesoId = proc.Id, updated = true });
    }

    // -------- Helpers --------
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
            q = q.Where(r => (r.PlacaSnapshot ?? string.Empty).Contains(qtext) ||
                             (r.Observaciones ?? string.Empty).Contains(qtext));
        return q;
    }

    private static bool IsSameScopeIdem(string? current, string scope, string key)
        => !string.IsNullOrWhiteSpace(current) && string.Equals(current, $"{scope}:{key}", StringComparison.Ordinal);

    private static bool HasDifferentIdemSameScope(string? current, string scope, string key)
        => !string.IsNullOrWhiteSpace(current) && current!.StartsWith(scope + ":", StringComparison.Ordinal)
           && !string.Equals(current, $"{scope}:{key}", StringComparison.Ordinal);

    private static void SetScopedIdem(Recibo r, string scope, string key)
        => r.IdempotencyKey = $"{scope}:{key}";

    private static string ComputeReciboEtag(Recibo r)
    {
        long last = r.UltimaActualizacion.HasValue ? r.UltimaActualizacion.Value.ToUnixTimeSeconds() : 0L;
        int estado = r.Estado;
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

        _cache.Set(key, etag, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) });
        return etag;
    }
}
