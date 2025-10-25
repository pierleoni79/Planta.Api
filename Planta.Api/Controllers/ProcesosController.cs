// Ruta: /Planta.Api/Controllers/ProcesosController.cs | V1.1
using System;
using Planta.Application.Abstractions;     // IProcesosService
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Planta.Contracts.Procesos;          // ProcesarTrituracionRequest / ProcesoResultDto

namespace Planta.Api.Controllers;

[ApiController]
[Route("api/procesos")]
[Produces("application/json")]
[Tags("Procesos")]
public sealed class ProcesosController : ControllerBase
{
    private readonly IProcesosService _svc;
    public ProcesosController(IProcesosService svc) => _svc = svc;

    /// <summary>Procesa la trituración (Modo A: ε=1%, δ_min=0.1)</summary>
    [HttpPost("trituracion/{reciboId:guid}/procesar")]
    [ProducesResponseType(typeof(ProcesoResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Procesar([FromRoute] Guid reciboId, [FromBody] ProcesarTrituracionRequest body, CancellationToken ct)
    {
        try
        {
            var dto = await _svc.ProcesarTrituracionAsync(reciboId, body, ct);
            return dto.Cumple ? Ok(dto) : UnprocessableEntity(dto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }
    }
}
