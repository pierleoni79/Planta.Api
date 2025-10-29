// Ruta: /Planta.Application/Features/Recibos/CrearRecibo.cs | V1.0
#nullable enable
using MediatR;
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;
using Planta.Application.Common;
using Planta.Contracts.Recibos;
using Planta.Data.Entities;
using Planta.Domain.Recibos;

namespace Planta.Application.Features.Recibos;

public sealed record CrearReciboCommand(CrearReciboRequest Body, string? IdempotencyKeyHeader, string? Usuario)
    : IRequest<CrearReciboResult>;

public sealed record CrearReciboResult(CrearReciboResponse Response, string ETag, bool Created, bool Idempotent);

public sealed class CrearReciboHandler : IRequestHandler<CrearReciboCommand, CrearReciboResult>
{
    private readonly IPlantaDbContext _db;
    public CrearReciboHandler(IPlantaDbContext db) => _db = db;

    public async Task<CrearReciboResult> Handle(CrearReciboCommand c, CancellationToken ct)
    {
        var body = c.Body;
        var scope = string.IsNullOrWhiteSpace(body.IdempotencyScope) ? "create" : body.IdempotencyScope.Trim();
        var key = !string.IsNullOrWhiteSpace(body.IdempotencyKey) ? body.IdempotencyKey!.Trim() : c.IdempotencyKeyHeader;
        if (string.IsNullOrWhiteSpace(key))
            throw new ValidationAppException("recibos/create/idempotency", "IdempotencyKey es requerido.");

        var composed = $"{scope}:{key}";

        var existing = await _db.Query<Recibo>()
            .AsNoTracking()
            .Where(r => r.IdempotencyKey == composed)
            .Select(r => new { r.Id, r.Consecutivo, r.UltimaActualizacion, r.FechaCreacion, r.Estado })
            .FirstOrDefaultAsync(ct);

        if (existing is not null)
        {
            var etagExisting = EtagHelper.ForRecibo(existing.Consecutivo, existing.UltimaActualizacion, existing.FechaCreacion, existing.Estado);
            return new CrearReciboResult(
                new CrearReciboResponse { ReciboId = existing.Id, Estado = (ReciboEstado)existing.Estado, ETag = etagExisting },
                etagExisting, Created: false, Idempotent: true);
        }

        if (!body.Cantidad.HasValue || body.Cantidad.Value <= 0)
            throw new ValidationAppException("recibos/create/cantidad", "Cantidad debe ser > 0.");

        if (body.Destino == DestinoTipo.ClienteDirecto && !body.ClienteId.HasValue)
            throw new ValidationAppException("recibos/create/cliente", "ClienteId es requerido para destino ClienteDirecto.");

        var estadoInicial = body.Destino == DestinoTipo.Planta ? ReciboEstado.EnTransito_Planta : ReciboEstado.EnTransito_Cliente;
        var rec = new Recibo
        {
            EmpresaId = body.EmpresaId,
            VehiculoId = body.VehiculoId,
            MaterialId = body.MaterialId,
            AlmacenOrigenId = body.AlmacenOrigenId,
            ClienteId = body.ClienteId,
            ConductorId = body.ConductorId,
            PlacaSnapshot = string.IsNullOrWhiteSpace(body.PlacaSnapshot) ? null : body.PlacaSnapshot.Trim(),
            ConductorNombreSnapshot = string.IsNullOrWhiteSpace(body.ConductorNombreSnapshot) ? null : body.ConductorNombreSnapshot.Trim(),
            DestinoTipo = (byte)body.Destino,
            Estado = (int)estadoInicial,
            UsuarioCreador = string.IsNullOrWhiteSpace(c.Usuario) ? "system" : c.Usuario!,
            Observaciones = string.IsNullOrWhiteSpace(body.ObservacionesIniciales) ? "[Creación]" : $"[Creación] {body.ObservacionesIniciales.Trim()}",
            Cantidad = body.Cantidad.Value,
            IdempotencyKey = composed,
            Activo = true
        };

        await _db.AddAsync(rec, ct);
        await _db.SaveChangesAsync(ct);

        var saved = await _db.Query<Recibo>()
            .AsNoTracking()
            .Where(x => x.Id == rec.Id)
            .Select(x => new { x.Id, x.Consecutivo, x.UltimaActualizacion, x.FechaCreacion, x.Estado })
            .FirstAsync(ct);

        var etag = EtagHelper.ForRecibo(saved.Consecutivo, saved.UltimaActualizacion, saved.FechaCreacion, saved.Estado);

        return new CrearReciboResult(
            new CrearReciboResponse { ReciboId = saved.Id, Estado = (ReciboEstado)saved.Estado, ETag = etag },
            etag, Created: true, Idempotent: false);
    }
}
