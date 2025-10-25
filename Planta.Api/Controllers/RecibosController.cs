// Ruta: /Planta.Api/Controllers/RecibosController.cs | V1.4
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;
using Planta.Application.Features.Recibos.Checkin;
using Planta.Contracts.Common;
using Planta.Contracts.Recibos;

namespace Planta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Recibos")]
public sealed class RecibosController : ControllerBase
{
    private readonly IRecibosService _svc;
    private readonly IMediator _mediator;

    public RecibosController(IRecibosService svc, IMediator mediator)
    {
        _svc = svc;
        _mediator = mediator;
    }

    // -------- Helpers comunes (alineados con CatalogosController) --------
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
        foreach (var t in raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            if (t == "*" || string.Equals(t.Trim(), etag, StringComparison.Ordinal))
                return true;
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

    /// <summary>Grid paginado de recibos con filtros básicos.</summary>
    [HttpGet("grid")]
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any, VaryByQueryKeys =
        new[] { "page", "pageSize", "empresaId", "estado", "clienteId", "desde", "hasta", "search" })]
    [ProducesResponseType(typeof(PagedResult<ReciboListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async Task<IActionResult> Grid(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? empresaId = null,
        [FromQuery] int? estado = null,
        [FromQuery] int? clienteId = null,
        [FromQuery] DateTimeOffset? desde = null,
        [FromQuery] DateTimeOffset? hasta = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        NormalizePage(ref page, ref pageSize);

        // ✅ PagedRequest usa 'Q'
        var req = new PagedRequest { Page = page, PageSize = pageSize, Q = search };

        var result = await _svc.ListarAsync(req, empresaId, estado, clienteId, desde, hasta, search, ct);
        return WithEtagOk(result, maxAgeSeconds: 30);
    }

    /// <summary>Detalle de un recibo.</summary>
    [HttpGet("{id:guid}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
    [ProducesResponseType(typeof(ReciboDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener([FromRoute] Guid id, CancellationToken ct)
    {
        var dto = await _svc.ObtenerAsync(id, ct);
        if (dto is null) return NotFound();
        return WithEtagOk(dto, maxAgeSeconds: 60);
    }

    /// <summary>Crea un recibo (idempotente si envías Idempotency-Key).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReciboDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear([FromBody] CrearReciboRequest body, CancellationToken ct)
    {
        var idemHeader = Request.Headers["Idempotency-Key"].ToString();
        var user = User?.Identity?.Name ?? "api";
        try
        {
            var dto = await _svc.CrearAsync(body, string.IsNullOrWhiteSpace(idemHeader) ? null : idemHeader, user, ct);
            return CreatedAtAction(nameof(Obtener), new { id = dto.Id }, dto);
        }
        catch (ArgumentException aex)
        {
            return UnprocessableEntity(aex.Message);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException?.Message?.Contains("UQ_Recibo_Empresa_Consecutivo", StringComparison.OrdinalIgnoreCase) == true
               || ex.InnerException?.Message?.Contains("IX_Recibo_Empresa_Consecutivo", StringComparison.OrdinalIgnoreCase) == true)
        {
            return Conflict("Ya existe un recibo con el mismo (EmpresaId, Consecutivo).");
        }
    }

    /// <summary>Cambia el estado del recibo con validación de transiciones.</summary>
    [HttpPut("{id:guid}/estado")]
    [ProducesResponseType(typeof(ReciboDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado([FromRoute] Guid id, [FromBody] CambiarEstadoRequest body, CancellationToken ct)
    {
        try
        {
            var dto = await _svc.CambiarEstadoAsync(id, body.NuevoEstado, body.Comentario, ct);
            return Ok(dto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Transición inválida", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>📍 Check-in en planta (EnTransito_Planta ⇒ EnPatioPlanta). Guarda GPS/nota.</summary>
    [HttpPost("{id:guid}/checkin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Checkin([FromRoute] Guid id, [FromBody] CheckinRequest body, CancellationToken ct)
    {
        await _mediator.Send(new Command(id, body), ct);
        return NoContent();
    }
}
