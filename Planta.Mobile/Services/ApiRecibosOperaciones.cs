//Ruta: /Planta.Mobile/Services/ApiRecibosOperaciones.cs | V1.0
#nullable enable
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Planta.Mobile.Services;

public sealed class ApiRecibosOperaciones : IApiRecibosOperaciones
{
    private readonly IApiClientFactory _factory;
    private readonly IEtagStore _etags;
    private readonly ICacheStore _cache;
    public ApiRecibosOperaciones(IApiClientFactory factory, IEtagStore etags, ICacheStore cache)
    { _factory = factory; _etags = etags; _cache = cache; }

    private static void AddIdem(HttpRequestMessage req, Guid key)
        => req.Headers.TryAddWithoutValidation("Idempotency-Key", key.ToString());

    private async Task RunWithIfMatchAsync(Guid id, string route, string currentEtag, CancellationToken ct)
    {
        using var http = _factory.Create();
        var req = new HttpRequestMessage(HttpMethod.Post, $"api/recibos/{id}/{route}")
        {
            Content = JsonContent.Create(new { })
        };
        req.Headers.IfMatch.Add(new EntityTagHeaderValue(currentEtag, true));
        AddIdem(req, Guid.NewGuid());
        var rsp = await http.SendAsync(req, ct).ConfigureAwait(false);
        if (rsp.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
            throw new InvalidOperationException("Versión desactualizada (412). Actualiza el Recibo y reintenta.");
        rsp.EnsureSuccessStatusCode();
        // invalidar caché local del detalle para forzar refetch
        var key = $"recibos:{id}";
        await _cache.SetAsync<object>(key, new { }); // marca mínima; opcionalmente eliminar archivo
    }

    public Task EjecutarCheckInAsync(Guid id, string currentEtag, Guid idem, CancellationToken ct)
    { return RunWithIfMatchAsync(id, "checkin", currentEtag, ct); }

    public Task EjecutarDescargaInicioAsync(Guid id, string currentEtag, Guid idem, CancellationToken ct)
    { return RunWithIfMatchAsync(id, "descargainicio", currentEtag, ct); }

    public Task EjecutarDescargaFinAsync(Guid id, string currentEtag, Guid idem, CancellationToken ct)
    { return RunWithIfMatchAsync(id, "descargafin", currentEtag, ct); }
}