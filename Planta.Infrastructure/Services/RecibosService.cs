// Ruta: /Planta.Infrastructure/Services/RecibosService.cs | V1.9
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Planta.Application.Abstractions;
using Planta.Contracts.Common;
using Planta.Contracts.Recibos;
using Planta.Data.Context;
using Planta.Data.Entities;
using Planta.Infrastructure.Options;

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

        public async Task<PagedResult<ReciboListItemDto>> ListarAsync(
            PagedRequest req,
            int? empresaId, int? estado, int? clienteId,
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

        public async Task<ReciboDetailDto> CrearAsync(
            CrearReciboRequest req, string? idempotencyKeyHeader, string? usuario, CancellationToken ct)
        {
            if (req.Cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a 0.");
            if (req.DestinoTipo == 2 && !req.ClienteId.HasValue)
                throw new ArgumentException("ClienteId es obligatorio para destino ClienteDirecto.");

            // Idempotencia: usa header o body
            var idem = string.IsNullOrWhiteSpace(idempotencyKeyHeader) ? req.IdempotencyKey : idempotencyKeyHeader;
            if (!string.IsNullOrWhiteSpace(idem))
            {
                var existente = await _db.Set<Recibo>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == idem, ct);
                if (existente is not null)
                    return Map(existente);
            }

            // ---------- Resolver snapshots con EF read-only ----------
            var (placa, resolvedConductorId, conductorNombre) =
                await ResolverSnapshotsAsync(req.VehiculoId, req.ConductorId, ct);

            var ahora = DateTimeOffset.UtcNow;

            var entity = new Recibo
            {
                EmpresaId = req.EmpresaId,
                FechaCreacion = ahora,
                Estado = (byte)(req.DestinoTipo == 1 ? ReciboEstado.EnTransito_Planta : ReciboEstado.EnTransito_Cliente),
                DestinoTipo = req.DestinoTipo,
                VehiculoId = req.VehiculoId,
                ConductorId = resolvedConductorId,
                ClienteId = req.ClienteId,
                MaterialId = req.MaterialId,
                Observaciones = req.Observaciones,
                UltimaActualizacion = ahora,
                Activo = true,
                Cantidad = req.Cantidad,
                AlmacenOrigenId = req.AlmacenOrigenId,
                // snapshots
                PlacaSnapshot = placa,
                ConductorNombreSnapshot = conductorNombre,
                UsuarioCreador = string.IsNullOrWhiteSpace(usuario) ? "api" : usuario,
                IdempotencyKey = string.IsNullOrWhiteSpace(idem) ? null : idem
            };

            _db.Add(entity);
            await _db.SaveChangesAsync(ct);

            // Autogenerar folio físico (si está habilitado)
            if (_opt.AutoGenerarFolioFisico && string.IsNullOrWhiteSpace(entity.ReciboFisicoNumero))
            {
                var serie = string.IsNullOrWhiteSpace(_opt.SerieFolio) ? "RF" : _opt.SerieFolio!.Trim();
                entity.ReciboFisicoNumero = $"{serie}-{entity.Consecutivo:D7}";
                entity.ReciboFisicoNumeroNorm = entity.ReciboFisicoNumero.ToUpperInvariant();
                await _db.SaveChangesAsync(ct);
            }

            return Map(entity);
        }

        public async Task<ReciboDetailDto> CambiarEstadoAsync(
            Guid id, ReciboEstado nuevo, string? comentario, CancellationToken ct)
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

            await _db.SaveChangesAsync(ct);
            return Map(r);
        }

        // -------- Helpers --------
        private async Task<(string? placa, int? conductorId, string? conductorNombre)>
            ResolverSnapshotsAsync(int vehiculoId, int? conductorIdRequest, CancellationToken ct)
        {
            await using var ro = await _roFactory.CreateDbContextAsync(ct);

            // Placa del vehículo
            var placa = await ro.Vehiculos
                .AsNoTracking()
                .Where(v => v.Id == vehiculoId)
                .Select(v => v.Placa)
                .FirstOrDefaultAsync(ct);

            // Conductor (si no vino en el request): último registro (abierto primero)
            int? conductorId = conductorIdRequest;
            if (!conductorId.HasValue)
            {
                conductorId = await ro.VehiculoConductorHist
                    .AsNoTracking()
                    .Where(h => h.VehiculoId == vehiculoId)
                    .OrderByDescending(h => h.Hasta == null)  // abiertos primero
                    .ThenByDescending(h => h.Hasta)
                    .ThenByDescending(h => h.Desde)
                    .Select(h => (int?)h.ConductorId)
                    .FirstOrDefaultAsync(ct);
            }

            // Nombre del conductor (si hay id)
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
            // op.Recibo no tiene "Unidad"
            Observaciones = r.Observaciones
        };
    }
}
