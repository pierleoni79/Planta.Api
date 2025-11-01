// Ruta: /Planta.Api/Controllers/HealthController.cs | V1.0
#nullable enable
using Asp.Versioning;

namespace Planta.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/health")]
public sealed class HealthController : ControllerBase
{
    /// <summary>Ping.</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new { ok = true, nowUtc = DateTime.UtcNow });
}
