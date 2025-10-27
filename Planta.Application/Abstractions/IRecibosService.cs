#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Planta.Contracts.Common;
using Planta.Contracts.Recibos;

namespace Planta.Application.Abstractions;

/// <summary>
/// Servicio de aplicación para Recibos, con soporte de Idempotencia por scope y ETag (If-Match).
/// </summary>
public interface IRecibosService
{
    Task<PagedResult<ReciboListItemDto>> ListarAsync(
        PagedRequest req,
        int? empresaId, ReciboEstado? estado, int? clienteId,
        DateTimeOffset? desde, DateTimeOffset? hasta,
        string? search, CancellationToken ct);

    Task<ReciboDetailDto?> ObtenerAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Crea un recibo. Si (scope:key) ya fue ejecutado, devuelve el existente
    /// y marca el resultado como idempotente.
    /// </summary>
    /// <param name="req">Debe incluir IdempotencyScope/IdempotencyKey.</param>
    /// <param name="usuario">Usuario que crea (se mapea a UsuarioCreador en BD).</param>
    /// <returns>(response, idempotent)</returns>
    Task<(CrearReciboResponse Response, bool Idempotent)> CrearAsync(
        CrearReciboRequest req,
        string? usuario,
        CancellationToken ct);

    /// <summary>
    /// Cambio de estado genérico (uso administrativo). Requiere If-Match para concurrencia.
    /// Devuelve DTO y nuevo ETag.
    /// </summary>
    Task<(ReciboDetailDto Dto, string ETag)> CambiarEstadoAsync(
        Guid id,
        ReciboEstado nuevo,
        string? comentario,
        string? ifMatch,
        CancellationToken ct);

    /// <summary>
    /// Marca check-in en planta: EnTransito_Planta → Descargando.
    /// Aplica orden: Idempotencia(scope:key) → If-Match/ETag → reglas → persistir(SetScopedIdem) → 200.
    /// Devuelve DTO, nuevo ETag y si fue idempotente.
    /// </summary>
    Task<(ReciboDetailDto Dto, string ETag, bool Idempotent)> CheckinAsync(
        Guid id,
        string? comentario,
        string? gps,              // si registras ubicación en ReciboEstadoLog
        string? ifMatch,          // ETag del cliente
        string idempotencyScope,  // p.ej. "checkin"
        string idempotencyKey,    // único por intento
        CancellationToken ct);
}
