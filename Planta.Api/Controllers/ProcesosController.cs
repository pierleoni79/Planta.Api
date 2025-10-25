// Ruta: /Planta.Api/Controllers/ProcesosController.cs | V1.0
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Planta.Application.Features.Procesos.Trituracion.Procesar;
using Planta.Contracts.Procesos;

namespace Planta.Api.Controllers;

[ApiController]
[Route("api/procesos")]
public sealed class ProcesosController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProcesosController(IMediator mediator) => _mediator = mediator;

    /// <summary>Procesa la trituración (Modo A: ε=1%, δ_min=0.1)</summary>
    [HttpPost("trituracion/{reciboId:guid}/procesar")]
    [ProducesResponseType(typeof(ProcesoResultDto), 200)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Procesar(Guid reciboId, [FromBody] ProcesarTrituracionRequest body, CancellationToken ct)
    {
        var dto = await _mediator.Send(new Command(reciboId, body), ct);
        return dto.Cumple ? Ok(dto) : UnprocessableEntity(dto);
    }
}
