// Ruta: /Planta.Mobile/Services/Api/TransporteApi.cs | V1.5-fix (implementa interfaz + alias + 404→null)
#nullable enable
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Planta.Contracts.Transporte;

namespace Planta.Mobile.Services.Api;

public sealed class TransporteApi : ITransporteApi
{
    private const string V = "/api/v1";
    private readonly IApiClient _api;

    public TransporteApi(IApiClient api) => _api = api;

    // Compat: alias al nombre usado en otras partes (si alguien llama By, redirige a Por)
    public Task<TransporteResolucionDto?> ResolverByPlacaAsync(string placa, CancellationToken ct = default)
        => ResolverPorPlacaAsync(placa, ct);

    public async Task<TransporteResolucionDto?> ResolverPorPlacaAsync(string placa, CancellationToken ct = default)
    {
        var qp = Uri.EscapeDataString((placa ?? string.Empty).Trim().ToUpperInvariant());
        try
        {
            return await _api.GetJsonAsync<TransporteResolucionDto>($"{V}/transporte/resolver-by-placa?placa={qp}", ct);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // La API devuelve 404 cuando no existe la placa → representamos como null
            return null;
        }
    }

    public async Task<IReadOnlyList<VehiculoFavoritoDto>> ListarFavoritosAsync(int empresaId, int max, CancellationToken ct = default)
    {
        try
        {
            var dto = await _api.GetJsonAsync<IReadOnlyList<VehiculoFavoritoDto>>(
                $"{V}/transporte/favoritos?empresaId={empresaId}&max={max}", ct);
            return dto ?? Array.Empty<VehiculoFavoritoDto>();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Array.Empty<VehiculoFavoritoDto>();
        }
    }

    public async Task<bool> ToggleFavoritoAsync(int vehiculoId, bool esFavorito, CancellationToken ct = default)
    {
        var (resp, _) = await _api.PostJsonAsync<object, object?>(
            $"{V}/transporte/toggle-favorito",
            new { VehiculoId = vehiculoId, EsFavorito = esFavorito },
            ct);
        return resp.IsSuccessStatusCode;
    }
}
