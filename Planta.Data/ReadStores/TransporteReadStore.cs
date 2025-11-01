// Ruta: /Planta.Data/ReadStores/TransporteReadStore.cs | V1.6-implements-fix
#nullable enable
using System.Data;
using Microsoft.Data.SqlClient;
using Planta.Application.Transporte.Abstractions;   // ← interfaz del ReadStore
using Planta.Contracts.Transporte;
using Planta.Data.Abstractions;

namespace Planta.Data.ReadStores;

public sealed class TransporteReadStore : ITransporteReadStore
{
    private readonly ISqlConnectionFactory _factory;
    public TransporteReadStore(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<VehiculoFavoritoDto>> ListarFavoritosAsync(int empresaId, int max, CancellationToken ct)
    {
        const string sql = @"
        SELECT TOP (@max)
               v.Id           AS VehiculoId,
               v.Placa        AS Placa,
               c.Nombre       AS ConductorNombre,
               CAST(NULL AS datetimeoffset) AS UltimoUso, -- tipado explícito
               v.EsFavorito   AS EsFavorito
          FROM tpt.Vehiculo v
          LEFT JOIN tpt.VehiculoConductorHist h ON h.VehiculoId = v.Id AND h.Hasta IS NULL
          LEFT JOIN tpt.Conductor c ON c.Id = h.ConductorId
         WHERE v.EmpresaId = @empresaId AND v.Activo = 1
         ORDER BY v.EsFavorito DESC, v.Id DESC";

        await using var con = await _factory.CreateOpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@empresaId", SqlDbType.Int) { Value = empresaId });
        cmd.Parameters.Add(new SqlParameter("@max", SqlDbType.Int) { Value = max });

        var list = new List<VehiculoFavoritoDto>();
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new VehiculoFavoritoDto(
                VehiculoId: rd.GetInt32(0),
                Placa: rd.GetString(1),
                ConductorNombre: rd.IsDBNull(2) ? null : rd.GetString(2),
                UltimoUso: rd.IsDBNull(3) ? (DateTimeOffset?)null : rd.GetDateTimeOffset(3),
                EsFavorito: !rd.IsDBNull(4) && rd.GetBoolean(4)
            ));
        }
        return list;
    }

    public async Task<TransporteResolucionDto?> ResolverTransportePorPlacaAsync(string placa, CancellationToken ct)
    {
        const string sql = @"
        SELECT v.Id,
               v.Placa,
               v.ClaseVehiculoId,
               cv.Nombre        AS ClaseNombre,
               cv.CapacidadM3,
               v.Activo         AS VehiculoActivo,
               h.ConductorId,
               c.Nombre         AS ConductorNombreSnapshot,
               c.Activo         AS ConductorActivo,
               CONVERT(bit, CASE WHEN EXISTS (
                   SELECT 1 FROM tpt.VehiculoConductorHist h2 WHERE h2.VehiculoId = v.Id
               ) THEN 1 ELSE 0 END) AS TieneHistorial
          FROM tpt.Vehiculo v
          LEFT JOIN tpt.ClaseVehiculo cv        ON cv.Id = v.ClaseVehiculoId
          LEFT JOIN tpt.VehiculoConductorHist h ON h.VehiculoId = v.Id AND h.Hasta IS NULL
          LEFT JOIN tpt.Conductor c             ON c.Id = h.ConductorId
         WHERE v.Placa = @placa";

        var placaNorm = (placa ?? string.Empty).Trim().ToUpperInvariant();

        await using var con = await _factory.CreateOpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@placa", SqlDbType.NVarChar, 10) { Value = placaNorm });

        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        if (!await rd.ReadAsync(ct)) return null;

        return new TransporteResolucionDto
        {
            VehiculoId = rd.GetInt32(0),
            Placa = rd.GetString(1),
            ClaseVehiculoId = rd.IsDBNull(2) ? (int?)null : rd.GetInt32(2),
            ClaseNombre = rd.IsDBNull(3) ? null : rd.GetString(3),
            CapacidadM3 = rd.IsDBNull(4) ? (decimal?)null : rd.GetDecimal(4),
            VehiculoActivo = !rd.IsDBNull(5) && rd.GetBoolean(5),
            ConductorId = rd.IsDBNull(6) ? (int?)null : rd.GetInt32(6),
            ConductorNombreSnapshot = rd.IsDBNull(7) ? null : rd.GetString(7),
            ConductorActivo = rd.IsDBNull(8) ? (bool?)null : rd.GetBoolean(8),
            FuenteResolucion = rd.IsDBNull(6) ? "SinDatos" : "Historial",
            TieneHistorial = rd.GetBoolean(9)
        };
    }
}
