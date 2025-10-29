//Ruta: /Planta.Mobile/Services/ApiRecibos.cs | V1.0
#nullable enable
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Planta.Contracts.Recibos;

namespace Planta.Mobile.Services;

public sealed class ApiRecibos : IApiRecibos
{
    private readonly IApiClientFactory _factory;
    private readonly IEtagStore _etags;
    private readonly ICacheStore _cache;
    public ApiRecibos(IApiClientFactory factory, IEtagStore etags, ICacheStore cache)
    { _factory = factory; _etags = etags; _cache = cache; }

    public async Task<(IReadOnlyList<ReciboListItemDto> items, string? etag)> ListarAsync(string? estado, string? q, string? knownEtag, CancellationToken ct)
    {
        var key = $"recibos:list:{estado}:{q}";
        using var http = _factory.Create();

        var url = $"api/recibos?estado={Uri.EscapeDataString(estado ?? "")}&q={Uri.EscapeDataString(q ?? "")}";
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        var etag = knownEtag ?? _etags.Get(key);
        if (!string.IsNullOrWhiteSpace(etag)) req.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(etag, true));

        var rsp = await http.SendAsync(req, ct).ConfigureAwait(false);
        if (rsp.StatusCode == HttpStatusCode.NotModified)
        {
            var cached = await _cache.GetAsync<List<ReciboListItemDto>>(key).ConfigureAwait(false) ?? new();
            return (cached, etag);
        }
        rsp.EnsureSuccessStatusCode();
        var list = await rsp.Content.ReadFromJsonAsync<List<ReciboListItemDto>>(cancellationToken: ct).ConfigureAwait(false) ?? new();
        var newEtag = rsp.Headers.ETag?.ToString();
        if (!string.IsNullOrWhiteSpace(newEtag)) { _etags.Set(key, newEtag!); await _cache.SetAsync(key, list); }
        return (list, newEtag);
    }

    public async Task<(ReciboDetailDto dto, string? etag)> ObtenerAsync(Guid id, string? knownEtag, CancellationToken ct)
    {
        var key = $"recibos:{id}";
        using var http = _factory.Create();

        var req = new HttpRequestMessage(HttpMethod.Get, $"api/recibos/{id}");
        var etag = knownEtag ?? _etags.Get(key);
        if (!string.IsNullOrWhiteSpace(etag)) req.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(etag, true));

        var rsp = await http.SendAsync(req, ct).ConfigureAwait(false);
        if (rsp.StatusCode == HttpStatusCode.NotModified)
        {
            var cached = await _cache.GetAsync<ReciboDetailDto>(key).ConfigureAwait(false);
            if (cached is null) throw new InvalidOperationException("304 pero sin caché local: sincroniza nuevamente.");
            return (cached, etag);
        }
        rsp.EnsureSuccessStatusCode();
        var dto = await rsp.Content.ReadFromJsonAsync<ReciboDetailDto>(cancellationToken: ct).ConfigureAwait(false)
                  ?? throw new InvalidOperationException("Respuesta vacía");
        var newEtag = rsp.Headers.ETag?.ToString();
        if (!string.IsNullOrWhiteSpace(newEtag)) { _etags.Set(key, newEtag!); await _cache.SetAsync(key, dto); }
        return (dto, newEtag);
    }
}