// Ruta: /Planta.Api/Controllers/CatalogosController.cs | V1.10
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Planta.Application.Abstractions;
using Planta.Contracts.Common;
using Planta.Contracts.Catalogos;
using Planta.Contracts.Transporte;

namespace Planta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Catalogos")]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogoRepository _repo;

    public CatalogosController(ICatalogoRepository repo) => _repo = repo;

    // ---------- Helpers ----------
    private static void NormalizePage(ref int page, ref int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = PagedRequest.DefaultPageSize;
        if (pageSize > PagedRequest.MaxPageSize) pageSize = PagedRequest.MaxPageSize;
    }

    private static string ComputeEtag(object payload)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false });
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = Convert.ToBase64String(SHA256.HashData(bytes));
        return $"W/\"{hash}\"";
    }

    private static bool MatchesIfNoneMatch(HttpRequest req, string etag)
    {
        var raw = req.Headers["If-None-Match"].ToString();
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var tokens = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var t in tokens)
        {
            var token = t.Trim();
            if (token == "*" || string.Equals(token, etag, StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    private IActionResult WithEtagOk(object payload, int maxAgeSeconds = 60)
    {
        var etag = ComputeEtag(payload);

        if (MatchesIfNoneMatch(Request, etag))
        {
            Response.Headers.ETag = etag;
            Response.Headers["Cache-Control"] = $"public,max-age={maxAgeSeconds}";
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers.ETag = etag;
        Response.Headers["Cache-Control"] = $"public,max-age={maxAgeSeconds}";
        return Ok(payload);
    }

    // ---------- Productos ----------
    [HttpGet("productos")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "search", "orderBy", "desc" })]
    [ProducesResponseType(typeof(PagedResult<ProductoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Productos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? orderBy = "nombre",
        [FromQuery] bool desc = false)
    {
        NormalizePage(ref page, ref pageSize);
        var req = new PagedRequest { Page = page, PageSize = pageSize, Q = search };
        var result = await _repo.ListarProductosAsync(req, search, orderBy, desc, HttpContext.RequestAborted);
        return WithEtagOk(result);
    }

    // ---------- Unidades ----------
    [HttpGet("unidades")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "search" })]
    [ProducesResponseType(typeof(PagedResult<UnidadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Unidades(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        NormalizePage(ref page, ref pageSize);
        var req = new PagedRequest { Page = page, PageSize = pageSize, Q = search };
        var result = await _repo.ListarUnidadesAsync(req, search, HttpContext.RequestAborted);
        return WithEtagOk(result);
    }

    // ---------- Vehículos ----------
    [HttpGet("vehiculos")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "search", "claseId", "soloActivos" })]
    [ProducesResponseType(typeof(PagedResult<VehiculoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Vehiculos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] int? claseId = null,
        [FromQuery] bool soloActivos = true)
    {
        NormalizePage(ref page, ref pageSize);
        var req = new PagedRequest { Page = page, PageSize = pageSize, Q = search };
        var result = await _repo.ListarVehiculosAsync(req, search, claseId, soloActivos, HttpContext.RequestAborted);
        return WithEtagOk(result);
    }

    // ---------- Autocomplete vehículos por placa (con último conductor) ----------
    /// <summary>
    /// Autocompleta por placa y devuelve { vehiculoId, placa, conductorId, conductorNombre }.
    /// </summary>
    [HttpGet("vehiculos/autocomplete")]
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "q", "take" })]
    [ProducesResponseType(typeof(VehiculoAutocompleteDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async Task<IActionResult> AutocompleteVehiculos(
        [FromQuery] string? q = null,
        [FromQuery] int take = 10,
        CancellationToken ct = default)
    {
        if (take <= 0) take = 10;
        if (take > 50) take = 50; // límite sano

        var result = await _repo.AutocompletarVehiculosAsync(q, take, ct);
        return WithEtagOk(result, maxAgeSeconds: 30);
    }

    // ---------- Conductores ----------
    [HttpGet("conductores")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "search", "soloActivos" })]
    [ProducesResponseType(typeof(PagedResult<ConductorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Conductores(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool soloActivos = true)
    {
        NormalizePage(ref page, ref pageSize);
        var req = new PagedRequest { Page = page, PageSize = pageSize, Q = search };
        var result = await _repo.ListarConductoresAsync(req, search, soloActivos, HttpContext.RequestAborted);
        return WithEtagOk(result);
    }

    // ---------- Plantas ----------
    [HttpGet("plantas")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "search" })]
    [ProducesResponseType(typeof(PagedResult<PlantaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Plantas(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        NormalizePage(ref page, ref pageSize);
        var req = new PagedRequest { Page = page, PageSize = pageSize, Q = search };
        var result = await _repo.ListarPlantasAsync(req, search, HttpContext.RequestAborted);
        return WithEtagOk(result);
    }

    // ---------- Almacenes ----------
    [HttpGet("almacenes")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "search", "plantaId" })]
    [ProducesResponseType(typeof(PagedResult<AlmacenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Almacenes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] int? plantaId = null)
    {
        NormalizePage(ref page, ref pageSize);
        var req = new PagedRequest { Page = page, PageSize = pageSize, Q = search };
        var result = await _repo.ListarAlmacenesAsync(req, search, plantaId, HttpContext.RequestAborted);
        return WithEtagOk(result);
    }

    // ---------- Clientes ----------
    [HttpGet("clientes")]
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "q", "page", "pageSize" })]
    [ProducesResponseType(typeof(PagedResult<ClienteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async Task<IActionResult> Clientes(
        [FromQuery] string? q = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        NormalizePage(ref page, ref pageSize);
        var req = new PagedRequest { Page = page, PageSize = pageSize, Q = q };
        var result = await _repo.ListarClientesAsync(req, q, ct);
        return WithEtagOk(result, maxAgeSeconds: 30);
    }

    /// <summary>Obtiene un cliente por Id.</summary>
    [HttpGet("clientes/{id:int}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClientePorId([FromRoute] int id, CancellationToken ct = default)
    {
        var dto = await _repo.ObtenerClientePorIdAsync(id, ct);
        if (dto is null) return NotFound();
        return WithEtagOk(dto, maxAgeSeconds: 60);
    }
}
