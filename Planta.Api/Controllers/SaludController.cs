// Ruta: /Planta.Api/Controllers/SaludController.cs | V1.0
using Microsoft.AspNetCore.Mvc;

namespace Planta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaludController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "OK" });
}
