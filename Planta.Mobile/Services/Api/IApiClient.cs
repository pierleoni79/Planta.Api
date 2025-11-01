// Ruta: /Planta.Mobile/Services/Api/IApiClient.cs | V1.2-fix
using System.Net.Http;

namespace Planta.Mobile.Services.Api
{
    public interface IApiClient
    {
        string BaseUrl { get; set; }
        void SetBearer(string? token);

        // Lecturas
        Task<(HttpResponseMessage response, string? body)> GetAsync(string path, CancellationToken ct = default);
        Task<T?> GetJsonAsync<T>(string path, CancellationToken ct = default);

        // Escrituras (POST JSON)
        // 1) Crudo (cuerpo como string para diagnosticar respuestas)
        Task<(HttpResponseMessage response, string? body)> PostJsonRawAsync<TReq>(string path, TReq payload, CancellationToken ct = default);

        // 2) Tipado (req/res diferentes)
        Task<(HttpResponseMessage response, TRes? body)> PostJsonAsync<TReq, TRes>(string path, TReq payload, CancellationToken ct = default);
    }
}
