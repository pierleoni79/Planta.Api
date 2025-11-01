// Ruta: /Planta.Api/Controllers/InfoController.cs | V1.0
#nullable enable
using Asp.Versioning;

namespace Planta.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/info")]
public sealed class InfoController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        name = "Planta.Api",
        version = "1.0",
        env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
        nowUtc = DateTime.UtcNow
    });
}
