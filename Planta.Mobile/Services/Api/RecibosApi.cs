// Ruta: /Planta.Mobile/Services/Api/RecibosApi.cs | V1.4-fix (v1 paths + CreateFullAsync)
using System.Text;
using Planta.Contracts.Common;
using Planta.Contracts.Enums;
using Planta.Contracts.Recibos;
using Planta.Contracts.Recibos.Queries;
using Planta.Contracts.Recibos.Requests;
using Planta.Mobile.Models.Recibos;

namespace Planta.Mobile.Services.Api;

public interface IRecibosApi
{
    // 🧩 Legacy (aún útil si el backend devuelve ReciboDto)
    Task<Guid?> CrearAsync(ReciboCreateRequest create, CancellationToken ct = default);

    // 🆕 Firma alineada con el VM: (form, idempotencyKey, ct)
    Task<Guid> CreateFullAsync(NuevoReciboForm form, string idempotencyKey, CancellationToken ct = default);

    Task<bool> VincularOrigenAsync(VincularOrigenRequest req, CancellationToken ct = default);
    Task<bool> VincularTransporteAsync(VincularTransporteRequest req, CancellationToken ct = default);
    Task<bool> RegistrarMaterialAsync(RegistrarMaterialRequest req, CancellationToken ct = default);

    Task<PagedResult<ReciboListItemDto>?> ListarAsync(ReciboListFilter filter, PagedRequest paging, CancellationToken ct = default);
    Task<PagedResult<ReciboListItemDto>?> ListarEnTransitoAsync(int empresaId, CancellationToken ct = default);
    Task<ReciboDto?> ObtenerAsync(Guid id, CancellationToken ct = default);
}

public sealed class RecibosApi : IRecibosApi
{
    private const string V = "/api/v1";
    private readonly IApiClient _api;
    private readonly AppState _state;

    public RecibosApi(IApiClient api, AppState state)
    {
        _api = api;
        _state = state;
    }

    // -------- Crear (legacy) --------
    public async Task<Guid?> CrearAsync(ReciboCreateRequest create, CancellationToken ct = default)
    {
        var (resp, dto) = await _api.PostJsonAsync<ReciboCreateRequest, ReciboDto>($"{V}/recibos", create, ct);
        if (resp.IsSuccessStatusCode && dto is not null) return dto.Id;

        if (resp.IsSuccessStatusCode && dto is null)
        {
            // algunos backends devuelven el Guid como texto plano
            var (r2, body) = await _api.PostJsonRawAsync($"{V}/recibos", create, ct);
            if (!string.IsNullOrWhiteSpace(body) && Guid.TryParse(body, out var gid))
                return gid;
        }
        return null;
    }

    // -------- Crear (nueva firma con idempotencia) --------
    public async Task<Guid> CreateFullAsync(NuevoReciboForm form, string idempotencyKey, CancellationToken ct = default)
    {
        // Si tu IApiClient soporta headers, ideal sería enviarlo como "Idempotency-Key".
        // Para compatibilidad amplia lo pasamos en querystring (el backend debe aceptarlo).
        var path = $"{V}/recibos?idem={Uri.EscapeDataString(idempotencyKey ?? string.Empty)}";

        // Intento 1: esperando ReciboDto
        var (resp, dto) = await _api.PostJsonAsync<NuevoReciboForm, ReciboDto>(path, form, ct);
        if (resp.IsSuccessStatusCode && dto is not null)
            return dto.Id;

        // Intento 2: texto plano con el Guid
        var (r2, body) = await _api.PostJsonRawAsync(path, form, ct);
        if (r2.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(body) && Guid.TryParse(body, out var gid))
            return gid;

        // Error controlado
        var reason = dto is null ? $"HTTP {(int)resp.StatusCode}" : "respuesta inesperada";
        throw new InvalidOperationException($"No se pudo crear el recibo (CreateFullAsync): {reason}.");
    }

    // -------- Acciones de flujo --------
    public Task<bool> VincularOrigenAsync(VincularOrigenRequest req, CancellationToken ct = default)
        => PostBool($"{V}/recibos/vincular-origen", req, ct);

    public Task<bool> VincularTransporteAsync(VincularTransporteRequest req, CancellationToken ct = default)
        => PostBool($"{V}/recibos/vincular-transporte", req, ct);

    public Task<bool> RegistrarMaterialAsync(RegistrarMaterialRequest req, CancellationToken ct = default)
        => PostBool($"{V}/recibos/registrar-material", req, ct);

    private async Task<bool> PostBool<T>(string path, T payload, CancellationToken ct)
    {
        var (r, _) = await _api.PostJsonAsync<T, object?>(path, payload, ct);
        return r.IsSuccessStatusCode;
    }

    // -------- Consultas --------
    public Task<PagedResult<ReciboListItemDto>?> ListarAsync(ReciboListFilter filter, PagedRequest paging, CancellationToken ct = default)
    {
        var sb = new StringBuilder($"{V}/recibos?empresaId={filter.EmpresaId}&page={paging.Page}&pageSize={paging.PageSize}");
        if (filter.Estado is ReciboEstado est)
            sb.Append("&estado=").Append((int)est);

        return _api.GetJsonAsync<PagedResult<ReciboListItemDto>>(sb.ToString(), ct);
    }

    public Task<PagedResult<ReciboListItemDto>?> ListarEnTransitoAsync(int empresaId, CancellationToken ct = default)
    {
        var filter = new ReciboListFilter(EmpresaId: empresaId, Estado: ReciboEstado.EnTransitoPlanta);
        var paging = new PagedRequest(1, 20);
        return ListarAsync(filter, paging, ct);
    }

    public Task<ReciboDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
        => _api.GetJsonAsync<ReciboDto>($"{V}/recibos/{id}", ct);
}
