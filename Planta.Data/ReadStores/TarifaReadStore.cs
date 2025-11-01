// Ruta: /Planta.Data/ReadStores/TarifaReadStore.cs | V1.1 (fix: using de interfaz de Application + alias DestinoTipo)
#nullable enable
using System.Data;
using Microsoft.Data.SqlClient;
using Planta.Application.Tarifas.Abstractions;     // ← interfaz vive en Application
using Planta.Contracts.Tarifas;
using Planta.Data.Abstractions;
// Evita ambigüedad con Domain.Enums.DestinoTipo
using DestinoTipoApi = Planta.Contracts.Enums.DestinoTipo;

namespace Planta.Data.ReadStores;

public sealed class TarifaReadStore(ISqlConnectionFactory factory) : ITarifaReadStore
{
    public async Task<TarifaVigenteDto> ObtenerTarifaVigenteAsync(
        int claseVehiculoId,
        int materialId,
        DestinoTipoApi destino,
        int? clienteId,
        int? plantaId,
        CancellationToken ct)
    {
        await using var con = await factory.CreateOpenAsync(ct);

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
            return new TarifaVigenteDto(null, null, 0m, int.MaxValue); // sin tarifa

        int? tarifaId = rd.IsDBNull(0) ? null : rd.GetInt32(0);
        string? unidad = rd.IsDBNull(1) ? null : rd.GetString(1);
        decimal precio = rd.IsDBNull(2) ? 0m : rd.GetDecimal(2);
        int prioridad = rd.IsDBNull(3) ? int.MaxValue : rd.GetInt32(3);

        return new TarifaVigenteDto(tarifaId, unidad, precio, prioridad);
    }
}
