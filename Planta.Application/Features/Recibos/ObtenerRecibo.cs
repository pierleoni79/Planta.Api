// Ruta: /Planta.Application/Features/Recibos/ObtenerRecibo.cs | V1.1
#nullable enable
using MediatR;
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;
using Planta.Application.Common;
using Planta.Contracts.Recibos;
using Planta.Data.Entities;
using Planta.Domain.Recibos;

namespace Planta.Application.Features.Recibos;

public sealed record ObtenerReciboQuery(Guid Id, string? IfNoneMatch)
    : IRequest<ObtenerReciboResult>;

public sealed record ObtenerReciboResult(ReciboDetailDto? Dto, string ETag, bool NotModified);

public sealed class ObtenerReciboHandler : IRequestHandler<ObtenerReciboQuery, ObtenerReciboResult>
{
    private readonly IPlantaDbContext _db;
    public ObtenerReciboHandler(IPlantaDbContext db) => _db = db;

    public async Task<ObtenerReciboResult> Handle(ObtenerReciboQuery q, CancellationToken ct)
    {
        var r = await _db.Query<Recibo>()
            .Where(x => x.Id == q.Id)
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
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (r is null) throw new KeyNotFoundException("Recibo no encontrado.");

        var etag = EtagHelper.ForRecibo(r.Consecutivo, r.UltimaActualizacion, r.FechaCreacion, r.Estado);
        if (!string.IsNullOrWhiteSpace(q.IfNoneMatch) && string.Equals(q.IfNoneMatch, etag, StringComparison.Ordinal))
            return new ObtenerReciboResult(null, etag, true);

        var dto = new ReciboDetailDto
        {
            Id = r.Id,
            EmpresaId = r.EmpresaId,
            Consecutivo = r.Consecutivo,
            FechaCreacion = r.FechaCreacion,
            Estado = (ReciboEstado)r.Estado,
            VehiculoId = r.VehiculoId,
            Placa = r.Placa,
            ClienteId = r.ClienteId,
            Cantidad = r.Cantidad,
            ConductorId = r.ConductorId,
            ConductorNombreSnapshot = r.ConductorNombreSnapshot,
            Observaciones = r.Observaciones
        };
        return new ObtenerReciboResult(dto, etag, false);
    }
}
