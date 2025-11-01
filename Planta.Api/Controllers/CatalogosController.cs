// Ruta: /Planta.Api/Controllers/CatalogosController.cs | V1.6-fix-cat-unidad
#nullable enable
using System.Data;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Planta.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/catalogos")]
[ApiVersion("1.0")]
public sealed class CatalogosController : ControllerBase
{
    private readonly string _cnn;

    public CatalogosController(IConfiguration config)
        => _cnn = config.GetConnectionString("SqlServer")!;

    // GET /api/v1/catalogos/unidades
    [HttpGet("unidades")]
    [ResponseCache(Duration = 60, VaryByHeader = "If-None-Match")]
    public async Task<IActionResult> GetUnidades(CancellationToken ct)
    {
        await using var con = new SqlConnection(_cnn);
        await con.OpenAsync(ct);

        // --- ETag calculado sobre el contenido actual de cat.Unidad ---
        var etag = await GetUnidadesEtagAsync(con, ct);
        var ifNoneMatch = Request.Headers.TryGetValue("If-None-Match", out var v) ? v.ToString() : null;

        if (!string.IsNullOrEmpty(ifNoneMatch) && string.Equals(ifNoneMatch, etag, StringComparison.Ordinal))
        {
            Response.Headers.ETag = etag;
            return StatusCode(StatusCodes.Status304NotModified);
        }

        const string sql = @"
            SELECT Id, Codigo, Nombre
            FROM cat.Unidad WITH (NOLOCK)
            ORDER BY Nombre;";

        var list = new List<UnidadDto>();
        await using (var cmd = new SqlCommand(sql, con))
        {
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;
            await using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct))
            {
                list.Add(new UnidadDto
                {
                    Id = rd.GetInt32(0),
                    Codigo = rd.IsDBNull(1) ? null : rd.GetString(1),
                    Nombre = rd.GetString(2)
                });
            }
        }

        Response.Headers.ETag = etag;
        return Ok(list);
    }

    private static async Task<string> GetUnidadesEtagAsync(SqlConnection con, CancellationToken ct)
    {
        const string sql = @"
            SELECT Etag = CONCAT('W/""',
                CONVERT(varchar(40), CHECKSUM_AGG(BINARY_CHECKSUM(Id, ISNULL(Codigo,''), ISNULL(Nombre,'')))),
            '""')
            FROM cat.Unidad WITH (NOLOCK);";

        await using var cmd = new SqlCommand(sql, con);
        var obj = await cmd.ExecuteScalarAsync(ct);
        var tag = obj?.ToString();
        return string.IsNullOrWhiteSpace(tag) ? "W/\"empty\"" : tag!;
    }

    public sealed record UnidadDto
    {
        public int Id { get; init; }
        public string? Codigo { get; init; }
        public string Nombre { get; init; } = default!;
    }
}
