// Ruta: /Planta.Api/Controllers/RecibosController.cs | V3.0
#nullable enable
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Planta.Application.Common;
using Planta.Application.Features.Recibos;
using Planta.Contracts.Recibos;

namespace Planta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RecibosController : ControllerBase
{
    private readonly IMediator _mediator;
    public RecibosController(IMediator mediator) => _mediator = mediator;

    // POST: crear
    [HttpPost]
    [ProducesResponseType(typeof(CrearReciboResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(CrearReciboResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Crear([FromBody] CrearReciboRequest body, CancellationToken ct)
    {
        try
        {
            var idempHeader = Request.Headers["Idempotency-Key"].ToString();
            var user = User?.Identity?.Name ?? User?.FindFirst("preferred_username")?.Value;
            var result = await _mediator.Send(new CrearReciboCommand(body, idempHeader, user), ct);
            Response.Headers["ETag"] = result.ETag;

            var location = Url.Action(nameof(Obtener), new { id = result.Response.ReciboId });
            if (result.Created) return Created(location!, result.Response);

            Response.Headers["X-Idempotent"] = "true";
            return Ok(result.Response);
        }
        catch (ValidationAppException ex) { return ValidationProblem(title: ex.Code, detail: ex.Message); }
    }

    // GET: lista simple (con 304)
    [HttpGet]
    [ProducesResponseType(typeof(ReciboListItemDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        int? empresaId = null, int? vehiculoId = null, int? clienteId = null, int? materialId = null,
        DateTimeOffset? desde = null, DateTimeOffset? hasta = null, string? q = null,
        CancellationToken ct = default)
    {
        var inm = Request.Headers["If-None-Match"].ToString();
        var res = await _mediator.Send(new ListarRecibosQuery(empresaId, vehiculoId, clienteId, materialId, desde, hasta, q, inm), ct);
        Response.Headers["ETag"] = res.ETag;
        if (res.NotModified) return StatusCode(StatusCodes.Status304NotModified);
        return Ok(res.Items);
    }

    // GET: detalle (con 304)
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken ct = default)
    {
        try
        {
            var inm = Request.Headers["If-None-Match"].ToString();
            var res = await _mediator.Send(new ObtenerReciboQuery(id, inm), ct);
            Response.Headers["ETag"] = res.ETag;
            if (res.NotModified) return StatusCode(StatusCodes.Status304NotModified);
            return Ok(res.Dto);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // POST: check-in
    [HttpPost("{id:guid}/checkin")]
    public async Task<IActionResult> Checkin(Guid id, [FromBody] CheckinRequest body, CancellationToken ct = default)
    {
        try
        {
            var ifMatch = Request.Headers["If-Match"].ToString();
            var res = await _mediator.Send(new CheckInCommand(id, ifMatch, body.IdempotencyKey, body.Observaciones, body.Gps), ct);
            Response.Headers["ETag"] = res.ETag;
            if (res.Idempotent) Response.Headers["X-Idempotent"] = "true";
            return Ok(res.Dto);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (PreconditionFailedException ex) { return Problem(statusCode: 412, title: ex.Code, detail: ex.Message); }
        catch (ConflictException ex) { return Problem(statusCode: 409, title: ex.Code, detail: ex.Message); }
    }

    // POST: descarga inicio
    [HttpPost("{id:guid}/descargainicio")]
    public async Task<IActionResult> DescargaInicio(Guid id, [FromBody] DescargaInicioRequest body, CancellationToken ct = default)
    {
        try
        {
            var ifMatch = Request.Headers["If-Match"].ToString();
            var (dto, etag, idem) = await _mediator.Send(new DescargaInicioCommand(id, ifMatch, body.IdempotencyKey, body.Observaciones), ct);
            Response.Headers["ETag"] = etag;
            if (idem) Response.Headers["X-Idempotent"] = "true";
            return Ok(dto);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (PreconditionFailedException ex) { return Problem(statusCode: 412, title: ex.Code, detail: ex.Message); }
        catch (ConflictException ex) { return Problem(statusCode: 409, title: ex.Code, detail: ex.Message); }
        catch (ValidationAppException ex) { return ValidationProblem(title: ex.Code, detail: ex.Message); }
    }

    // POST: descarga fin
    [HttpPost("{id:guid}/descargafin")]
    public async Task<IActionResult> DescargaFin(Guid id, [FromBody] DescargaFinRequest body, CancellationToken ct = default)
    {
        try
        {
            var ifMatch = Request.Headers["If-Match"].ToString();
            var (dto, etag, procId, idem) = await _mediator.Send(new DescargaFinCommand(id, ifMatch, body.IdempotencyKey, body.Proceso, body.Observaciones), ct);
            Response.Headers["ETag"] = etag;
            if (idem) Response.Headers["X-Idempotent"] = "true";
            return Ok(new { dto.Id, dto.Estado, ProcesoId = procId });
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (PreconditionFailedException ex) { return Problem(statusCode: 412, title: ex.Code, detail: ex.Message); }
        catch (ConflictException ex) { return Problem(statusCode: 409, title: ex.Code, detail: ex.Message); }
        catch (ValidationAppException ex) { return ValidationProblem(title: ex.Code, detail: ex.Message); }
    }
}
