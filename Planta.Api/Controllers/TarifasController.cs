// Ruta: /Planta.Api/Controllers/TarifasController.cs | V1.0
#nullable enable
namespace Planta.Api.Controllers;

using System.Data;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Planta.Contracts.Enums;
using Planta.Contracts.Tarifas;

[ApiController]
[Route("api/v{version:apiVersion}/tarifas")]
[ApiVersion("1.0")]
public sealed class TarifasController : ControllerBase
{
    private readonly string _cnn;

    public TarifasController(IConfiguration config)
        => _cnn = config.GetConnectionString("SqlServer")!;

    // ----------------------------------------------------------------
    // GET /api/v1/tarifas/vigente?claseVehiculoId=&materialId=&destino=&clienteId?=&plantaId?=
    // destino: Planta | ClienteDirecto
    // ----------------------------------------------------------------
    [HttpGet("vigente")]
    public async Task<ActionResult<TarifaVigenteDto>> Vigente(
        [FromQuery] int claseVehiculoId,
        [FromQuery] int materialId,
        [FromQuery] DestinoTipo destino,
        [FromQuery] int? clienteId,
        [FromQuery] int? plantaId,
        CancellationToken ct)
    {
        if (claseVehiculoId <= 0 || materialId <= 0)
            return UnprocessableEntity("Parámetros requeridos: claseVehiculoId, materialId.");

        if (destino == DestinoTipo.ClienteDirecto && (clienteId is null or <= 0))
            return UnprocessableEntity("clienteId es requerido cuando destino=ClienteDirecto.");

        if (destino == DestinoTipo.Planta && (plantaId is null or <= 0))
            return UnprocessableEntity("plantaId es requerido cuando destino=Planta.");

        await using var con = new SqlConnection(_cnn);
        await con.OpenAsync(ct);

        // Permitimos tarifas "genéricas" (ClienteId/PlantaId NULL) y preferimos las específicas.
        const string sql = """
            DECLARE @hoy date = CAST(SYSDATETIMEOFFSET() AS date);

            SELECT TOP 1
               t.Id        AS TarifaId,
               t.Unidad    AS Unidad,
               t.Precio    AS Precio,
               t.Prioridad AS Prioridad,
               CASE
                 WHEN @destino = 2 /*ClienteDirecto*/ AND t.ClienteId IS NOT NULL THEN 2
                 WHEN @destino = 1 /*Planta*/         AND t.PlantaId  IS NOT NULL THEN 2
                 ELSE 1
               END AS Especificidad
            FROM tpt.TarifaConductor t
            WHERE t.Activo = 1
              AND t.ClaseVehiculoId = @clase
              AND t.ProductoId      = @mat
              AND t.VigenciaDesde  <= @hoy
              AND (t.VigenciaHasta IS NULL OR @hoy <= t.VigenciaHasta)
              AND (
                   (@destino = 2 AND (t.ClienteId = @cid OR t.ClienteId IS NULL))
                OR (@destino = 1 AND (t.PlantaId  = @pid OR t.PlantaId  IS NULL))
              )
            ORDER BY t.Prioridad ASC, Especificidad DESC
            """;

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new("@clase", SqlDbType.Int) { Value = claseVehiculoId });
        cmd.Parameters.Add(new("@mat", SqlDbType.Int) { Value = materialId });
        cmd.Parameters.Add(new("@destino", SqlDbType.Int) { Value = (int)destino });
        cmd.Parameters.Add(new("@cid", SqlDbType.Int) { Value = (object?)clienteId ?? DBNull.Value });
        cmd.Parameters.Add(new("@pid", SqlDbType.Int) { Value = (object?)plantaId ?? DBNull.Value });

        using var rd = await cmd.ExecuteReaderAsync(ct);
        if (!await rd.ReadAsync(ct))
            return Ok(new TarifaVigenteDto(null, null, 0m, int.MaxValue)); // sin tarifa

        int? tarifaId = rd.IsDBNull(0) ? null : rd.GetInt32(0);
        string? unidad = rd.IsDBNull(1) ? null : rd.GetString(1);
        decimal precio = rd.IsDBNull(2) ? 0m : rd.GetDecimal(2);
        int prioridad = rd.IsDBNull(3) ? int.MaxValue : rd.GetInt32(3);

        return Ok(new TarifaVigenteDto(tarifaId, unidad, precio, prioridad));
    }
}
