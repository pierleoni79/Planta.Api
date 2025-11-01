// Ruta: /Planta.Mobile/Services/Api/CatalogosApi.cs | +V1.1 (tipado)
using Planta.Contracts.Config;
using Planta.Contracts.CRM;

namespace Planta.Mobile.Services.Api;

public interface ICatalogosApi
{
    Task<List<ClienteDto>?> GetClientesAsync(CancellationToken ct = default);
    Task<List<PlantaDto>?> GetPlantasAsync(CancellationToken ct = default);
}

public sealed class CatalogosApi : ICatalogosApi
{
    private readonly IApiClient _api;
    public CatalogosApi(IApiClient api) => _api = api;

    public Task<List<ClienteDto>?> GetClientesAsync(CancellationToken ct = default)
        => _api.GetJsonAsync<List<ClienteDto>>("/api/catalogos/clientes?activo=true", ct);

    public Task<List<PlantaDto>?> GetPlantasAsync(CancellationToken ct = default)
        => _api.GetJsonAsync<List<PlantaDto>>("/api/catalogos/plantas?activo=true", ct);
}
