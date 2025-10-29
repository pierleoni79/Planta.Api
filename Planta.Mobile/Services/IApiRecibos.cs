//Ruta: /Planta.Mobile/Services/IApiRecibos.cs | V1.0
#nullable enable
using System.Net.Http.Headers;
using Planta.Contracts.Recibos;

namespace Planta.Mobile.Services;

public interface IApiRecibos
{
    Task<(IReadOnlyList<ReciboListItemDto> items, string? etag)> ListarAsync(string? estado, string? q, string? knownEtag, CancellationToken ct);
    Task<(ReciboDetailDto dto, string? etag)> ObtenerAsync(Guid id, string? knownEtag, CancellationToken ct);
}