// Ruta: /Planta.Mobile/Services/Api/VehiculosApi.cs | +V1.1
using Planta.Contracts.Transporte;

namespace Planta.Mobile.Services.Api;

public interface IVehiculosApi { Task<VehiculoDto?> GetByPlacaAsync(string placa, CancellationToken ct = default); }
public sealed class VehiculosApi : IVehiculosApi
{
    private readonly IApiClient _api;
    public VehiculosApi(IApiClient api) => _api = api;

    public Task<VehiculoDto?> GetByPlacaAsync(string placa, CancellationToken ct = default)
        => _api.GetJsonAsync<VehiculoDto>($"/api/vehiculos/by-placa/{Uri.EscapeDataString(placa)}", ct);
}
