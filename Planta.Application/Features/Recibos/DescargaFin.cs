// Ruta: /Planta.Application/Features/Recibos/DescargaFin.cs | V1.0
#nullable enable
using MediatR;
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;
using Planta.Application.Common;
using Planta.Contracts.Recibos;
using Planta.Data.Entities;
using Planta.Domain.Recibos;
using System.Diagnostics;

namespace Planta.Application.Features.Recibos;

public sealed record DescargaFinCommand(
    Guid Id, string? IfMatch, string? IdempotencyKey,
    ProcesarTrituracionDto Proceso, string? Observaciones
) : IRequest<(ReciboDetailDto Dto, string ETag, Guid ProcesoId, bool Idempotent)>;

public sealed class DescargaFinHandler : IRequestHandler<DescargaFinCommand, (ReciboDetailDto Dto, string ETag, Guid ProcesoId, bool Idempotent)>
{
    private const string Scope = "descarga-fin";
    private readonly IPlantaDbContext _db;
    public DescargaFinHandler(IPlantaDbContext db) => _db = db;

    public async Task<(ReciboDetailDto Dto, string ETag, Guid ProcesoId, bool Idempotent)> Handle(DescargaFinCommand c, CancellationToken ct)
    {
        var rec = await _db.Query<Recibo>().FirstOrDefaultAsync(r => r.Id == c.Id, ct)
                  ?? throw new KeyNotFoundException("Recibo no encontrado.");

        if (!string.IsNullOrWhiteSpace(rec.IdempotencyKey) && rec.IdempotencyKey.StartsWith(Scope + ":", StringComparison.Ordinal))
        {
            if (!string.IsNullOrWhiteSpace(c.IdempotencyKey) && string.Equals($"{Scope}:{c.IdempotencyKey}", rec.IdempotencyKey, StringComparison.Ordinal))
            {
                var etExisting = EtagHelper.ForRecibo(rec.Consecutivo, rec.UltimaActualizacion, rec.FechaCreacion, rec.Estado);
                var lastProcId = await _db.Query<Proceso>().Where(p => p.ReciboId == rec.Id).OrderByDescending(p => p.CreadoEn).Select(p => p.Id).FirstOrDefaultAsync(ct);
                return (Project(rec), etExisting, lastProcId, true);
            }
            if (!string.IsNullOrWhiteSpace(c.IdempotencyKey))
                throw new ConflictException("recibos/idempotencia", "Operación ya ejecutada. Use la misma IdempotencyKey.");
        }

        var currentEtag = EtagHelper.ForRecibo(rec.Consecutivo, rec.UltimaActualizacion, rec.FechaCreacion, rec.Estado);
        if (!string.IsNullOrWhiteSpace(c.IfMatch) && !string.Equals(c.IfMatch, currentEtag, StringComparison.Ordinal))
            throw new PreconditionFailedException("recibos/etag", "If-Match no coincide.");

        if (rec.Estado != (int)ReciboEstado.Descargando)
            throw new ConflictException("recibos/estado-invalido", "El fin de descarga solo aplica cuando el recibo está 'Descargando'.");

        if (c.Proceso is null || c.Proceso.Salidas is null || c.Proceso.Salidas.Count == 0)
            throw new ValidationAppException("recibos/proceso", "Debe incluir al menos una salida.");

        var entrada = c.Proceso.Entrada.Cantidad;
        var totalSalidas = c.Proceso.Salidas.Sum(s => s.Cantidad);
        if (entrada <= 0 || totalSalidas <= 0)
            throw new ValidationAppException("recibos/proceso", "Entrada y salidas deben ser > 0.");

        var delta = Math.Abs(entrada - totalSalidas);
        var tolPct = c.Proceso.ToleranciaPct ?? 1.0m;
        var maxDelta = (entrada * tolPct) / 100m;
        if (delta > maxDelta)
            throw new ConflictException("recibos/proceso/descuadre", $"Δ={delta} m3 fuera de tolerancia ({tolPct}%).");

        var proc = new Proceso
        {
            ReciboId = rec.Id,
            RecetaId = c.Proceso.RecetaId,
            EntradaM3 = entrada,
            Observacion = string.IsNullOrWhiteSpace(c.Observaciones) ? null : c.Observaciones,
            CreadoEn = DateTimeOffset.UtcNow
        };

        await _db.AddAsync(proc, ct);
        await _db.SaveChangesAsync(ct);

        foreach (var s in c.Proceso.Salidas)
        {
            var det = new ProcesoDet { ProcesoId = proc.Id, ProductoId = s.ProductoId, CantidadM3 = s.Cantidad };
            await _db.AddAsync(det, ct);
        }

        rec.Estado = (int)ReciboEstado.Procesado;
        if (!string.IsNullOrWhiteSpace(c.IdempotencyKey)) rec.IdempotencyKey = $"{Scope}:{c.IdempotencyKey}";
        if (!string.IsNullOrWhiteSpace(c.Observaciones))
            rec.Observaciones = string.IsNullOrWhiteSpace(rec.Observaciones) ? c.Observaciones : $"{rec.Observaciones} | {c.Observaciones}";
        rec.UltimaActualizacion = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        var etag = EtagHelper.ForRecibo(rec.Consecutivo, rec.UltimaActualizacion, rec.FechaCreacion, rec.Estado);
        return (Project(rec), etag, proc.Id, false);
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
