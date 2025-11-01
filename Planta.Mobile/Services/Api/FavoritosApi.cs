// Ruta: /Planta.Mobile/Services/Api/FavoritosApi.cs | +V1.1
using Planta.Contracts.Transporte;

namespace Planta.Mobile.Services.Api;

public interface IFavoritosApi { Task<List<VehiculoDto>?> GetAsync(CancellationToken ct = default); }
public sealed class FavoritosApi : IFavoritosApi
{
    private readonly IApiClient _api;
    public FavoritosApi(IApiClient api) => _api = api;

    public Task<List<VehiculoDto>?> GetAsync(CancellationToken ct = default)
        => _api.GetJsonAsync<List<VehiculoDto>>("/api/vehiculos/favoritos", ct);
}
