// Ruta: /Planta.Mobile/Services/Api/ConductoresApi.cs | +V1.1
using Planta.Contracts.Transporte;

namespace Planta.Mobile.Services.Api;

public interface IConductoresApi { Task<List<ConductorDto>?> SearchAsync(string q, CancellationToken ct = default); }
public sealed class ConductoresApi : IConductoresApi
{
    private readonly IApiClient _api;
    public ConductoresApi(IApiClient api) => _api = api;

    public Task<List<ConductorDto>?> SearchAsync(string q, CancellationToken ct = default)
        => _api.GetJsonAsync<List<ConductorDto>>($"/api/conductores?activo=true&q={Uri.EscapeDataString(q)}");
}
