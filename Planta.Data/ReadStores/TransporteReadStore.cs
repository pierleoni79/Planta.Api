// Ruta: /Planta.Data/ReadStores/TransporteReadStore.cs | V1.7-fix (clamp max + parámetros seguros + ordinals + ct opcional + TOP 1)
#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Planta.Application.Transporte.Abstractions;
using Planta.Contracts.Transporte;
using Planta.Data.Abstractions;

namespace Planta.Data.ReadStores;

public sealed class TransporteReadStore : ITransporteReadStore
{
    private readonly ISqlConnectionFactory _factory;
    public TransporteReadStore(ISqlConnectionFactory factory) => _factory = factory;

    // --------- Favoritos ---------
    public async Task<IReadOnlyList<VehiculoFavoritoDto>> ListarFavoritosAsync(
        int empresaId,
        int max = 50,
        CancellationToken ct = default)
    {
        // capar valores fuera de rango
        var top = max <= 0 ? 50 : (max > 1000 ? 1000 : max);

        const string sql = @"
    SET NOCOUNT ON;
    SELECT TOP (@max)
           v.Id                             AS VehiculoId,
           v.Placa                          AS Placa,
           c.Nombre                         AS ConductorNombre,
           CAST(NULL AS datetimeoffset)     AS UltimoUso,     -- reservado para futuras métricas
           v.EsFavorito                     AS EsFavorito
      FROM tpt.Vehiculo v
      LEFT JOIN tpt.VehiculoConductorHist h ON h.VehiculoId = v.Id AND h.Hasta IS NULL
      LEFT JOIN tpt.Conductor c             ON c.Id = h.ConductorId
     WHERE v.EmpresaId = @empresaId AND v.Activo = 1
     ORDER BY v.EsFavorito DESC, v.Id DESC;";

        await using var con = await _factory.CreateOpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con)
        {
            CommandType = CommandType.Text
        };
        cmd.Parameters.Add(new SqlParameter("@empresaId", SqlDbType.Int) { Value = empresaId });
        cmd.Parameters.Add(new SqlParameter("@max", SqlDbType.Int) { Value = top });

        // Ordinals para lecturas seguras
        const int ORD_VehiculoId = 0;
        const int ORD_Placa = 1;
        const int ORD_CondNombre = 2;
        const int ORD_UltUso = 3;
        const int ORD_EsFavorito = 4;

        var list = new List<VehiculoFavoritoDto>(capacity: Math.Min(top, 1000));
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new VehiculoFavoritoDto(
                VehiculoId: rd.GetInt32(ORD_VehiculoId),
                Placa: rd.GetString(ORD_Placa),
                ConductorNombre: rd.IsDBNull(ORD_CondNombre) ? null : rd.GetString(ORD_CondNombre),
                UltimoUso: rd.IsDBNull(ORD_UltUso) ? (DateTimeOffset?)null : rd.GetDateTimeOffset(ORD_UltUso),
                EsFavorito: !rd.IsDBNull(ORD_EsFavorito) && rd.GetBoolean(ORD_EsFavorito)
            ));
        }
        return list;
    }

    // --------- Resolver por placa ---------
    public async Task<TransporteResolucionDto?> ResolverTransportePorPlacaAsync(
        string placa,
        CancellationToken ct = default)
    {
        // Normalización defensiva
        var placaNorm = (placa ?? string.Empty).Trim().ToUpperInvariant();
        if (placaNorm.Length == 0) return null;

        const string sql = @"
    SET NOCOUNT ON;
    SELECT TOP (1)
           v.Id,
           v.Placa,
           v.ClaseVehiculoId,
           cv.Nombre                       AS ClaseNombre,
           cv.CapacidadM3,
           v.Activo                        AS VehiculoActivo,
           h.ConductorId,
           c.Nombre                        AS ConductorNombreSnapshot,
           c.Activo                        AS ConductorActivo,
           CONVERT(bit, CASE WHEN EXISTS (
               SELECT 1 FROM tpt.VehiculoConductorHist h2 WHERE h2.VehiculoId = v.Id
           ) THEN 1 ELSE 0 END)           AS TieneHistorial
      FROM tpt.Vehiculo v
      LEFT JOIN tpt.ClaseVehiculo      cv ON cv.Id = v.ClaseVehiculoId
      LEFT JOIN tpt.VehiculoConductorHist h ON h.VehiculoId = v.Id AND h.Hasta IS NULL
      LEFT JOIN tpt.Conductor           c ON c.Id = h.ConductorId
     WHERE v.Placa = @placa
     ORDER BY v.Id DESC;";  // determinista si hubiera duplicados

        await using var con = await _factory.CreateOpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con)
        {
            CommandType = CommandType.Text
        };
        cmd.Parameters.Add(new SqlParameter("@placa", SqlDbType.NVarChar, 10) { Value = placaNorm });

        // Ordinals
        const int O_Id = 0;
        const int O_Placa = 1;
        const int O_ClaseId = 2;
        const int O_ClaseNombre = 3;
        const int O_Capacidad = 4;
        const int O_VehiculoActivo = 5;
        const int O_ConductorId = 6;
        const int O_ConductorNombre = 7;
        const int O_ConductorActivo = 8;
        const int O_TieneHist = 9;

        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        if (!await rd.ReadAsync(ct)) return null;

        return new TransporteResolucionDto
        {
            VehiculoId = rd.GetInt32(O_Id),
            Placa = rd.GetString(O_Placa),
            ClaseVehiculoId = rd.IsDBNull(O_ClaseId) ? (int?)null : rd.GetInt32(O_ClaseId),
            ClaseNombre = rd.IsDBNull(O_ClaseNombre) ? null : rd.GetString(O_ClaseNombre),
            CapacidadM3 = rd.IsDBNull(O_Capacidad) ? (decimal?)null : rd.GetDecimal(O_Capacidad),
            VehiculoActivo = !rd.IsDBNull(O_VehiculoActivo) && rd.GetBoolean(O_VehiculoActivo),
            ConductorId = rd.IsDBNull(O_ConductorId) ? (int?)null : rd.GetInt32(O_ConductorId),
            ConductorNombreSnapshot = rd.IsDBNull(O_ConductorNombre) ? null : rd.GetString(O_ConductorNombre),
            ConductorActivo = rd.IsDBNull(O_ConductorActivo) ? (bool?)null : rd.GetBoolean(O_ConductorActivo),
            FuenteResolucion = rd.IsDBNull(O_ConductorId) ? "SinDatos" : "Historial",
            TieneHistorial = rd.GetBoolean(O_TieneHist)
        };
    }


}