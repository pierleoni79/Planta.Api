// Ruta: /Planta.Infrastructure/Services/RecibosService.cs | V1.12
#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Planta.Application.Abstractions;
using Planta.Contracts.Common;
using Planta.Contracts.Recibos;
using Planta.Data.Context;
using Planta.Data.Entities;
using Planta.Infrastructure.Options;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Planta.Infrastructure.Services
{
    public sealed class RecibosService : IRecibosService
    {
        private readonly PlantaDbContext _db;
        private readonly RecibosOptions _opt;
        private readonly IDbContextFactory<TransporteReadDbContext> _roFactory;

        public RecibosService(
            PlantaDbContext db,
            IOptions<RecibosOptions> opt,
            IDbContextFactory<TransporteReadDbContext> roFactory)
        {
            _db = db;
            _opt = opt.Value;
            _roFactory = roFactory;
        }

        // ===== Listado (firma con ReciboEstado?) =====
        public async Task<PagedResult<ReciboListItemDto>> ListarAsync(
            PagedRequest req,
            int? empresaId, ReciboEstado? estado, int? clienteId,
            DateTimeOffset? desde, DateTimeOffset? hasta,
            string? search, CancellationToken ct)
        {
            var page = req.Page < 1 ? PagedRequest.DefaultPage : req.Page;
            var pageSize = req.PageSize < 1
                ? PagedRequest.DefaultPageSize
                : (req.PageSize > PagedRequest.MaxPageSize ? PagedRequest.MaxPageSize : req.PageSize);

            var term = string.IsNullOrWhiteSpace(search) ? req.Q : search;

            IQueryable<Recibo> q = _db.Set<Recibo>().AsNoTracking();

            if (empresaId.HasValue) q = q.Where(x => x.EmpresaId == empresaId.Value);
            if (estado.HasValue) q = q.Where(x => x.Estado == (byte)estado.Value);
            if (clienteId.HasValue) q = q.Where(x => x.ClienteId == clienteId.Value);
            if (desde.HasValue) q = q.Where(x => x.FechaCreacion >= desde.Value);
            if (hasta.HasValue) q = q.Where(x => x.FechaCreacion < hasta.Value);

            if (!string.IsNullOrWhiteSpace(term))
            {
                var s = term.Trim();
                q = q.Where(x =>
                    EF.Functions.Like(x.Observaciones ?? "", $"%{s}%") ||
                    EF.Functions.Like(x.PlacaSnapshot ?? "", $"%{s}%"));
            }

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(x => x.FechaCreacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ReciboListItemDto
                {
                    Id = x.Id,
                    EmpresaId = x.EmpresaId,
                    Consecutivo = x.Consecutivo,
                    FechaCreacion = x.FechaCreacion,
                    Estado = (ReciboEstado)x.Estado,
                    VehiculoId = x.VehiculoId,
                    Placa = x.PlacaSnapshot,
                    ClienteId = x.ClienteId,
                    Cantidad = x.Cantidad
                })
                .ToListAsync(ct);

            return new PagedResult<ReciboListItemDto>(items, page, pageSize, total);
        }

        public async Task<ReciboDetailDto?> ObtenerAsync(Guid id, CancellationToken ct)
        {
            var r = await _db.Set<Recibo>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            return r is null ? null : Map(r);
        }

        // ===== Crear (tupla: Response + Idempotent) =====
        public async Task<(CrearReciboResponse Response, bool Idempotent)> CrearAsync(
            CrearReciboRequest req, string? idempotencyKeyHeader, CancellationToken ct)
        {
            // Validaciones mínimas contrato
            if (!req.Cantidad.HasValue || req.Cantidad.Value <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.", nameof(req.Cantidad));
            if (req.Destino == DestinoTipo.ClienteDirecto && !req.ClienteId.HasValue)
                throw new ArgumentException("ClienteId es obligatorio para destino ClienteDirecto.", nameof(req.ClienteId));

            // Idempotencia create: scope + key
            var scope = string.IsNullOrWhiteSpace(req.IdempotencyScope) ? "create" : req.IdempotencyScope.Trim();
            var key = !string.IsNullOrWhiteSpace(req.IdempotencyKey) ? req.IdempotencyKey.Trim() : idempotencyKeyHeader;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("IdempotencyKey es requerido.", nameof(req.IdempotencyKey));
            var composed = $"{scope}:{key}";

            var existente = await _db.Set<Recibo>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == composed, ct);

            if (existente is not null)
            {
                var etExisting = ComputeEtag(existente);
                return (new CrearReciboResponse
                {
                    ReciboId = existente.Id,
                    Estado = (ReciboEstado)existente.Estado,
                    ETag = etExisting
                }, true);
            }

            // Snapshots (placa, conductor)
            var (placa, resolvedConductorId, conductorNombre) =
                await ResolverSnapshotsAsync(req.VehiculoId, req.ConductorId, ct);

            var ahora = DateTimeOffset.UtcNow;

            var entity = new Recibo
            {
                EmpresaId = req.EmpresaId,
                FechaCreacion = ahora, // la BD también lo setea por default; mantenemos coherencia
                Estado = (byte)(req.Destino == DestinoTipo.Planta
                                    ? ReciboEstado.EnTransito_Planta
                                    : ReciboEstado.EnTransito_Cliente),
                DestinoTipo = (byte)req.Destino,
                VehiculoId = req.VehiculoId,
                ConductorId = resolvedConductorId,
                ClienteId = req.ClienteId,
                MaterialId = req.MaterialId,
                Observaciones = string.IsNullOrWhiteSpace(req.ObservacionesIniciales)
                                    ? "[Creación]"
                                    : $"[Creación] {req.ObservacionesIniciales.Trim()}",
                UltimaActualizacion = ahora,
                Activo = true,
                Cantidad = req.Cantidad!.Value, // validado arriba
                AlmacenOrigenId = req.AlmacenOrigenId,
                PlacaSnapshot = placa,
                ConductorNombreSnapshot = conductorNombre,
                UsuarioCreador = "api",
                IdempotencyKey = composed
            };

            _db.Add(entity);
            await _db.SaveChangesAsync(ct);

            if (_opt.AutoGenerarFolioFisico && string.IsNullOrWhiteSpace(entity.ReciboFisicoNumero))
            {
                var serie = string.IsNullOrWhiteSpace(_opt.SerieFolio) ? "RF" : _opt.SerieFolio!.Trim();
                entity.ReciboFisicoNumero = $"{serie}-{entity.Consecutivo:D7}";
                entity.ReciboFisicoNumeroNorm = entity.ReciboFisicoNumero.ToUpperInvariant();
                await _db.SaveChangesAsync(ct);
            }

            var etag = ComputeEtag(entity);

            return (new CrearReciboResponse
            {
                ReciboId = entity.Id,
                Estado = (ReciboEstado)entity.Estado,
                ETag = etag
            }, false);
        }

        // ===== Cambiar estado (tupla: Dto + ETag) =====
        public async Task<(ReciboDetailDto Dto, string ETag)> CambiarEstadoAsync(
            Guid id, ReciboEstado nuevo, string? comentario, string? idempotencyKey, CancellationToken ct)
        {
            var r = await _db.Set<Recibo>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (r is null) throw new InvalidOperationException("Recibo no encontrado.");

            var actual = (ReciboEstado)r.Estado;
            var valida = (actual, nuevo) switch
            {
                (ReciboEstado.Borrador, ReciboEstado.EnTransito_Planta) => true,
                (ReciboEstado.Borrador, ReciboEstado.EnTransito_Cliente) => true,
                (ReciboEstado.EnTransito_Planta, ReciboEstado.EnPatioPlanta) => true,
                (ReciboEstado.EnPatioPlanta, ReciboEstado.Descargando) => true,
                (ReciboEstado.Descargando, ReciboEstado.Procesado) => true,
                (ReciboEstado.Procesado, ReciboEstado.Cerrado) => true,
                (_, ReciboEstado.Anulado) => true,
                _ => false
            };
            if (!valida)
                throw new InvalidOperationException($"Transición inválida: {actual} → {nuevo}");

            r.Estado = (byte)nuevo;
            r.UltimaActualizacion = DateTimeOffset.UtcNow;

            if (!string.IsNullOrWhiteSpace(comentario))
                r.Observaciones = string.IsNullOrWhiteSpace(r.Observaciones)
                    ? comentario
                    : $"{r.Observaciones} | {comentario}";

            if (!string.IsNullOrWhiteSpace(idempotencyKey))
                r.IdempotencyKey = $"estado:{idempotencyKey.Trim()}";

            await _db.SaveChangesAsync(ct);

            var dto = Map(r);
            var etag = ComputeEtag(r);
            return (dto, etag);
        }

        // ===== Check-in (tupla: Dto + ETag + Idempotent) =====
        // Nota: Validación If-Match/ETag puede hacerse arriba (API). Aquí devolvemos siempre el ETag actual.
        public async Task<(ReciboDetailDto Dto, string ETag, bool Idempotent)> CheckinAsync(
            Guid id, string? gps, string? comentario, string? idempotencyKey, string ifMatch, string etag, CancellationToken ct)
        {
            var r = await _db.Set<Recibo>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (r is null) throw new InvalidOperationException("Recibo no encontrado.");

            // Idempotencia por scope "checkin"
            var scope = "checkin";
            var key = idempotencyKey?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(r.IdempotencyKey) &&
                r.IdempotencyKey.Equals($"{scope}:{key}", StringComparison.Ordinal))
            {
                // ya ejecutado con misma key
                var et = ComputeEtag(r);
                return (Map(r), et, true);
            }
            if (!string.IsNullOrWhiteSpace(r.IdempotencyKey) &&
                r.IdempotencyKey.StartsWith(scope + ":", StringComparison.Ordinal) &&
                !r.IdempotencyKey.Equals($"{scope}:{key}", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Use la misma IdempotencyKey (scope checkin).");
            }

            var actual = (ReciboEstado)r.Estado;
            if (actual != ReciboEstado.EnTransito_Planta)
                throw new InvalidOperationException($"Transición inválida: {actual} → {ReciboEstado.EnPatioPlanta}");

            // (opcional) precondición If-Match sencilla
            var currentEtag = ComputeEtag(r);
            if (!string.IsNullOrWhiteSpace(ifMatch) && !string.Equals(ifMatch.Trim(), currentEtag, StringComparison.Ordinal))
                throw new InvalidOperationException("Precondition Failed (ETag).");

            r.Estado = (byte)ReciboEstado.EnPatioPlanta;
            r.UltimaActualizacion = DateTimeOffset.UtcNow;

            if (!string.IsNullOrWhiteSpace(comentario))
                r.Observaciones = string.IsNullOrWhiteSpace(r.Observaciones)
                    ? comentario
                    : $"{r.Observaciones} | {comentario}";

            _db.Set<ReciboEstadoLog>().Add(new ReciboEstadoLog
            {
                ReciboId = r.Id,
                Estado = (byte)ReciboEstado.EnPatioPlanta,
                Cuando = DateTimeOffset.UtcNow,
                Nota = comentario,
                GPS = gps
            });

            // set de idempotencia por scope
            if (!string.IsNullOrWhiteSpace(key))
                r.IdempotencyKey = $"{scope}:{key}";

            await _db.SaveChangesAsync(ct);

            var etagAfter = ComputeEtag(r);
            return (Map(r), etagAfter, false);
        }

        // -------- Helpers --------
        private async Task<(string? placa, int? conductorId, string? conductorNombre)>
            ResolverSnapshotsAsync(int vehiculoId, int? conductorIdRequest, CancellationToken ct)
        {
            await using var ro = await _roFactory.CreateDbContextAsync(ct);

            var placa = await ro.Vehiculos
                .AsNoTracking()
                .Where(v => v.Id == vehiculoId)
                .Select(v => v.Placa)
                .FirstOrDefaultAsync(ct);

            int? conductorId = conductorIdRequest;
            if (!conductorId.HasValue)
            {
                conductorId = await ro.VehiculoConductorHist
                    .AsNoTracking()
                    .Where(h => h.VehiculoId == vehiculoId)
                    .OrderByDescending(h => h.Hasta == null)
                    .ThenByDescending(h => h.Hasta)
                    .ThenByDescending(h => h.Desde)
                    .Select(h => (int?)h.ConductorId)
                    .FirstOrDefaultAsync(ct);
            }

            string? conductorNombre = null;
            if (conductorId.HasValue)
            {
                conductorNombre = await ro.Conductores
                    .AsNoTracking()
                    .Where(c => c.Id == conductorId.Value)
                    .Select(c => c.Nombre)
                    .FirstOrDefaultAsync(ct);
            }

            return (placa, conductorId, conductorNombre);
        }

        private static ReciboDetailDto Map(Recibo r) => new ReciboDetailDto
        {
            Id = r.Id,
            EmpresaId = r.EmpresaId,
            Consecutivo = r.Consecutivo,
            FechaCreacion = r.FechaCreacion,
            Estado = (ReciboEstado)r.Estado,
            DestinoTipo = r.DestinoTipo,
            VehiculoId = r.VehiculoId,
            ConductorId = r.ConductorId,
            Placa = r.PlacaSnapshot,
            ClienteId = r.ClienteId,
            MaterialId = r.MaterialId,
            AlmacenOrigenId = r.AlmacenOrigenId,
            Cantidad = r.Cantidad,
            Observaciones = r.Observaciones
        };

        // ETag consistente con API: W/"base64(consecutivo|timestamp|estado)"
        private static string ComputeEtag(Recibo r)
        {
            var stamp = r.UltimaActualizacion ?? r.FechaCreacion;
            var raw = $"{r.Consecutivo}|{stamp.UtcDateTime:o}|{r.Estado}";
            return $"W/\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(raw))}\"";
        }
    }
}
