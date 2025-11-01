// Ruta: /Planta.Api/Controllers/RecibosController.cs | V1.5
#nullable enable
namespace Planta.Api.Controllers;

using Asp.Versioning;
using Planta.Api.Utilities;
using Planta.Application.Recibos.Commands.ActualizarRecibo;
using Planta.Application.Recibos.Commands.CambiarEstado;
using Planta.Application.Recibos.Commands.CrearRecibo;
using Planta.Application.Recibos.Commands.RegistrarMaterial;
using Planta.Application.Recibos.Commands.VincularOrigen;
using Planta.Application.Recibos.Commands.VincularTransporte;
using Planta.Application.Recibos.Queries.ListarRecibos;
// Nota: NO importamos el namespace profundo de ObtenerRecibo para evitar confusión.
//       Usaremos el nombre totalmente calificado en la invocación.

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/recibos")]
public sealed class RecibosController : ControllerBase
{
    private readonly IMediator _mediator;
    public RecibosController(IMediator mediator) => _mediator = mediator;

    // ------------------- LISTADO -------------------
    /// <summary>Lista recibos con filtros y paginación.</summary>
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? empresaId = null,
        [FromQuery] Planta.Contracts.Enums.ReciboEstado? estado = null,
        [FromQuery] Planta.Contracts.Enums.DestinoTipo? destinoTipo = null,
        [FromQuery] DateTime? fechaDesdeUtc = null,
        [FromQuery] DateTime? fechaHastaUtc = null,
        [FromQuery] int? vehiculoId = null,
        [FromQuery] int? conductorId = null,
        [FromQuery] int? clienteId = null,
        [FromQuery] int? materialId = null,
        [FromQuery] string? search = null,
        [FromQuery(Name = "sort")] string? sortCsv = null,
        CancellationToken ct = default)
    {
        var sort = ParseSort(sortCsv);
        var query = new ReciboListQuery(
            new PagedRequest(page, pageSize, sort),
            new ReciboListFilter(empresaId, estado, destinoTipo, fechaDesdeUtc, fechaHastaUtc, vehiculoId, conductorId, clienteId, materialId, search)
        );
        var result = await _mediator.Send(new Planta.Application.Recibos.Queries.ListarRecibos.Query(query), ct);

        // ETag para el listado
        var listEtag = ETagUtils.ForList(result.Items.Select(x => x.ETag), result.TotalCount);
        if (Request.Headers.TryGetValue("If-None-Match", out var inm) && string.Equals(inm.ToString(), listEtag, StringComparison.Ordinal))
            return StatusCode(StatusCodes.Status304NotModified);

        Response.Headers.ETag = listEtag;
        return Ok(result);
    }

    // ------------------- DETALLE -------------------
    /// <summary>Obtiene un recibo por Id.</summary>
    [HttpGet("{id:guid}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken ct)
    {
        // Usamos el nombre totalmente calificado para evitar CS0246
        var dto = await _mediator.Send(new Planta.Application.Recibos.Queries.ObtenerRecibo.Query(id), ct);
        var etag = dto.ETag; // viene del ReadStore
        if (Request.Headers.TryGetValue("If-None-Match", out var inm) && string.Equals(inm.ToString(), etag, StringComparison.Ordinal))
            return StatusCode(StatusCodes.Status304NotModified);
        Response.Headers.ETag = etag;
        return Ok(dto);
    }

    // ------------------- CREAR -------------------
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ReciboCreateRequest body, CancellationToken ct)
    {
        // Calificación total para evitar ambigüedad con otros Command
        var dto = await _mediator.Send(new Planta.Application.Recibos.Commands.CrearRecibo.Command(body), ct);
        Response.Headers.ETag = dto.ETag;
        return CreatedAtAction(nameof(Obtener), new { id = dto.Id, version = "1.0" }, dto);
    }

    // ------------------- ACTUALIZAR -------------------
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ReciboUpdateRequest body, CancellationToken ct)
    {
        if (id != body.Id) return BadRequest("Id de ruta difiere del cuerpo");
        var dto = await _mediator.Send(new Planta.Application.Recibos.Commands.ActualizarRecibo.Command(body), ct);
        Response.Headers.ETag = dto.ETag;
        return Ok(dto);
    }

    // ------------------- CAMBIAR ESTADO -------------------
    [HttpPost("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoRequest body, CancellationToken ct)
    {
        if (id != body.Id) return BadRequest("Id de ruta difiere del cuerpo");
        var dto = await _mediator.Send(new Planta.Application.Recibos.Commands.CambiarEstado.Command(body), ct);
        Response.Headers.ETag = dto.ETag;
        return Ok(dto);
    }

    // ------------------- VINCULAR ORIGEN -------------------
    [HttpPost("{id:guid}/origen")]
    public async Task<IActionResult> VincularOrigen(Guid id, [FromBody] VincularOrigenRequest body, CancellationToken ct)
    {
        if (id != body.Id) return BadRequest("Id de ruta difiere del cuerpo");
        var dto = await _mediator.Send(new Planta.Application.Recibos.Commands.VincularOrigen.Command(body), ct);
        Response.Headers.ETag = dto.ETag;
        return Ok(dto);
    }

    // ------------------- VINCULAR TRANSPORTE -------------------
    [HttpPost("{id:guid}/transporte")]
    public async Task<IActionResult> VincularTransporte(Guid id, [FromBody] VincularTransporteRequest body, CancellationToken ct)
    {
        if (id != body.Id) return BadRequest("Id de ruta difiere del cuerpo");
        var dto = await _mediator.Send(new Planta.Application.Recibos.Commands.VincularTransporte.Command(body), ct);
        Response.Headers.ETag = dto.ETag;
        return Ok(dto);
    }

    private static IReadOnlyList<SortSpec> ParseSort(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return Array.Empty<SortSpec>();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                  .Select(s => s.StartsWith('-') ? new SortSpec(s[1..], true) : new SortSpec(s, false))
                  .ToList();
    }
}
