// Ruta: /Planta.Mobile/Services/Api/TarifasApi.cs | V1.0
#nullable enable
using System.Net.Http.Json;
using Planta.Contracts.Enums;
using Planta.Contracts.Tarifas;

namespace Planta.Mobile.Services.Api;

public sealed class TarifasApi
{
    private readonly HttpClient _http;

    public TarifasApi(HttpClient http) => _http = http;

    // GET /api/v1/tarifas/vigente?claseVehiculoId=..&materialId=..&destino=..&clienteId=..&plantaId=..
    public Task<TarifaVigenteDto> ObtenerVigenteAsync(
        int claseVehiculoId, int materialId, DestinoTipo destino, int? clienteId, int? plantaId, CancellationToken ct)
    {
        var url = $"api/v1/tarifas/vigente?claseVehiculoId={claseVehiculoId}&materialId={materialId}&destino={(int)destino}";
        if (clienteId is not null) url += $"&clienteId={clienteId}";
        if (plantaId is not null) url += $"&plantaId={plantaId}";
        return _http.GetFromJsonAsync<TarifaVigenteDto>(url, ct)!;
    }
}
