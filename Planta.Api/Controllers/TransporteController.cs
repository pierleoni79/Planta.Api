// Ruta: /Planta.Api/Controllers/TransporteController.cs | V1.8-fix (ValidationProblem claro)
#nullable enable
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Planta.Contracts.Transporte;
using Planta.Data.ReadStores;

namespace Planta.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/transporte")]
public sealed class TransporteController : ControllerBase
{
    private readonly TransporteReadStore _read;
    private readonly ILogger<TransporteController> _logger;

    public TransporteController(TransporteReadStore read, ILogger<TransporteController> logger)
    {
        _read = read;
        _logger = logger;
    }

    // GET: /api/v1/transporte/favoritos?empresaId=1&max=8
    [HttpGet("favoritos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VehiculoFavoritoDto>>> Favoritos(
        [FromQuery] int empresaId,
        [FromQuery] int max = 8,
        CancellationToken ct = default)
    {
        var list = await _read.ListarFavoritosAsync(empresaId, max, ct);
        return Ok(list ?? Enumerable.Empty<VehiculoFavoritoDto>());
    }

    // GET: /api/v1/transporte/resolver-by-placa?placa=QHK485
    [HttpGet("resolver-by-placa")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransporteResolucionDto>> ResolverPorPlaca(
        [FromQuery(Name = "placa")] string placa,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(placa))
        {
            ModelState.AddModelError("placa", "The placa field is required.");
            return ValidationProblem(ModelState); // ✅ sin ambigüedad
        }

        var dto = await _read.ResolverTransportePorPlacaAsync(placa.Trim().ToUpperInvariant(), ct);

        if (dto is null)
            return NotFound(new { message = $"No existe un vehículo con placa '{placa}' en tpt.Vehiculo." });

        return Ok(dto);
    }
}
