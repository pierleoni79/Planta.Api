// Ruta: /Planta.Data/Repositories/TransporteRepository.cs | V1.2 (EnsureVehiculoConductorAsync)
#nullable enable
using Microsoft.Data.SqlClient;
using Planta.Application.Transporte.Abstractions;
using Planta.Data.Abstractions;
using System.Data;

namespace Planta.Data.Repositories;

public sealed class TransporteRepository : ITransporteRepository
{
    private readonly ISqlConnectionFactory _factory; // usa el mismo factory que ya tiene tu clase

    public TransporteRepository(ISqlConnectionFactory factory)
        => _factory = factory;

    public async Task<bool> ToggleFavoritoAsync(int vehiculoId, bool esFavorito, CancellationToken ct)
    {
        // (método existente en tu repo)
        throw new NotImplementedException();
    }

    public async Task EnsureVehiculoConductorAsync(int vehiculoId, int conductorId, CancellationToken ct)
    {
        await using var con = await _factory.CreateOpenAsync(ct);

        var sql = @"
        -- Cerrar asignación abierta si corresponde a otro conductor
        IF EXISTS (SELECT 1
                   FROM tpt.VehiculoConductorHist WITH (UPDLOCK, HOLDLOCK)
                   WHERE VehiculoId = @vehiculoId AND Hasta IS NULL AND ConductorId <> @conductorId)
        BEGIN
            UPDATE tpt.VehiculoConductorHist
               SET Hasta = SYSDATETIMEOFFSET()
             WHERE VehiculoId = @vehiculoId AND Hasta IS NULL;
        END

        -- Abrir asignación si no hay una abierta con el mismo conductor
        IF NOT EXISTS (SELECT 1
                       FROM tpt.VehiculoConductorHist
                       WHERE VehiculoId = @vehiculoId AND ConductorId = @conductorId AND Hasta IS NULL)
        BEGIN
            INSERT INTO tpt.VehiculoConductorHist (VehiculoId, ConductorId, Desde)
            VALUES (@vehiculoId, @conductorId, SYSDATETIMEOFFSET());
        END";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@vehiculoId", SqlDbType.Int) { Value = vehiculoId });
        cmd.Parameters.Add(new SqlParameter("@conductorId", SqlDbType.Int) { Value = conductorId });
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
