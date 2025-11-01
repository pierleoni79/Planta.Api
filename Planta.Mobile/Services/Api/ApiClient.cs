// Ruta: /Planta.Mobile/Services/Api/ApiClient.cs | V1.3-dev
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Planta.Mobile.Services.Cache;

namespace Planta.Mobile.Services.Api;

public sealed class ApiClient : IApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IETagCacheService _etagCache;
    private readonly AppState _state;

    // Dejar vacío por defecto: usaremos el BaseAddress del named client "api"
    public string BaseUrl { get; set; } = string.Empty;

    private static readonly JsonSerializerOptions J = new() { PropertyNameCaseInsensitive = true };

    public ApiClient(IHttpClientFactory factory, IETagCacheService etagCache, AppState state)
    {
        _factory = factory;
        _etagCache = etagCache;
        _state = state;
    }

    public void SetBearer(string? token) => _state.SetToken(token);

    private HttpClient Create()
    {
        var c = _factory.CreateClient("api");

        // Solo aplica BaseUrl si te la cambiaron en runtime (Configuración)
        if (!string.IsNullOrWhiteSpace(BaseUrl))
        {
            if (c.BaseAddress?.ToString() != BaseUrl)
                c.BaseAddress = new Uri(BaseUrl);
        }

        if (!string.IsNullOrWhiteSpace(_state.BearerToken))
            c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _state.BearerToken);

        // Acept JSON por defecto
        if (!c.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[ApiClient] BaseAddress: {c.BaseAddress}");
#endif
        return c;
    }

    public async Task<(HttpResponseMessage response, string? body)> GetAsync(string path, CancellationToken ct = default)
    {
        using var c = Create();
        var url = new Uri(c.BaseAddress!, path).ToString();

        var etag = _etagCache.Get(url);
        if (!string.IsNullOrWhiteSpace(etag))
            c.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Parse(etag));

        var resp = await c.GetAsync(path, ct);
        string? body = null;

        if (resp.StatusCode == System.Net.HttpStatusCode.NotModified)
            return (resp, null);

        if (resp.Content is not null)
        {
            body = await resp.Content.ReadAsStringAsync(ct);
            if (resp.Headers.ETag is { } serverEtag)
                _etagCache.Put(url, serverEtag.ToString()); // guarda W/"..."
        }
        return (resp, body);
    }

    public async Task<(HttpResponseMessage response, string? body)> PostJsonRawAsync<TReq>(string path, TReq payload, CancellationToken ct = default)
    {
        using var c = Create();
        var json = JsonSerializer.Serialize(payload, J);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await c.PostAsync(path, content, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        return (resp, body);
    }

    public async Task<T?> GetJsonAsync<T>(string path, CancellationToken ct = default)
    {
        var (r, b) = await GetAsync(path, ct);
        if (!r.IsSuccessStatusCode || string.IsNullOrWhiteSpace(b)) return default;
        return JsonSerializer.Deserialize<T>(b, J);
    }

    public async Task<(HttpResponseMessage response, TRes? body)> PostJsonAsync<TReq, TRes>(string path, TReq payload, CancellationToken ct = default)
    {
        var (r, b) = await PostJsonRawAsync(path, payload, ct);
        var obj = (!r.IsSuccessStatusCode || string.IsNullOrWhiteSpace(b)) ? default : JsonSerializer.Deserialize<TRes>(b, J);
        return (r, obj);
    }
}
