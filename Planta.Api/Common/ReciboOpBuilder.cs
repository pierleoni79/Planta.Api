using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;
using Planta.Data.Entities;

namespace Planta.Api.Common;

/// Resultado de los “guards”
public sealed class ReciboOpResult
{
    public Recibo? Recibo { get; init; }
    public IActionResult? EarlyReturn { get; init; }   // si no es null, devuélvelo tal cual
    public bool ShouldReturn => EarlyReturn is not null;
}

/// Builder/fluent para unificar los checks de los endpoints de Recibos
public sealed class ReciboOpBuilder
{
    private readonly ControllerBase _ctl;
    private readonly IPlantaDbContext _db;

    private Recibo? _rec;
    private IActionResult? _early;

    private ReciboOpBuilder(ControllerBase ctl, IPlantaDbContext db)
    {
        _ctl = ctl;
        _db = db;
    }

    public static ReciboOpBuilder For(ControllerBase ctl, IPlantaDbContext db) => new(ctl, db);

    public ReciboOpBuilder RequireValidModelState()
    {
        if (!_ctl.ModelState.IsValid)
            _early = _ctl.ValidationProblem(_ctl.ModelState);
        return this;
    }

    public async Task<ReciboOpBuilder> LoadReciboAsync(Guid id, CancellationToken ct)
    {
        if (_early is not null) return this;

        _rec = await _db.Query<Recibo>().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (_rec is null) _early = _ctl.NotFound();
        return this;
    }

    public ReciboOpBuilder Idempotency(string scope, string key)
    {
        if (_early is not null || _rec is null) return this;

        if (IsSameScopeIdem(_rec.IdempotencyKey, scope, key))
        {
            _early = _ctl.Ok(new { id = _rec.Id, estado = (int)_rec.Estado, idempotent = true });
            return this;
        }
        if (HasDifferentIdemSameScope(_rec.IdempotencyKey, scope, key))
        {
            _early = _ctl.ConflictProblem("recibos/idempotencia", "Operación ya ejecutada",
                                          "Use la misma IdempotencyKey.");
        }
        return this;
    }

    public ReciboOpBuilder IfMatch()
    {
        if (_early is not null || _rec is null) return this;

        var ifMatch = _ctl.Request.Headers.IfMatch.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(ifMatch) &&
            !string.Equals(ifMatch, ComputeReciboEtag(_rec), StringComparison.Ordinal))
        {
            _early = _ctl.PreconditionFailedProblem("recibos/etag", "Precondición fallida", "If-Match no coincide.");
        }
        return this;
    }

    public ReciboOpResult Build()
        => new() { Recibo = _rec, EarlyReturn = _early };

    // ===== Helpers compartidos (idempotencia + ETag) =====
    public static bool IsSameScopeIdem(string? current, string scope, string key)
        => !string.IsNullOrWhiteSpace(current) &&
           string.Equals(current, $"{scope}:{key}", StringComparison.Ordinal);

    public static bool HasDifferentIdemSameScope(string? current, string scope, string key)
        => !string.IsNullOrWhiteSpace(current) &&
           current!.StartsWith(scope + ":", StringComparison.Ordinal) &&
           !string.Equals(current, $"{scope}:{key}", StringComparison.Ordinal);

    public static void SetScopedIdem(Recibo r, string scope, string key)
        => r.IdempotencyKey = $"{scope}:{key}";

    public static string ComputeReciboEtag(Recibo r)
    {
        long last = r.UltimaActualizacion?.ToUnixTimeSeconds() ?? 0L;
        int estado = r.Estado;
        return $"W/\"{r.Id:N}-{last}-{estado}\"";
    }
}
