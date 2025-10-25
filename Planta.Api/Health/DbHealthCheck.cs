// Ruta: /Planta.Api/Health/DbHealthCheck.cs | V1.0
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Planta.Data.Context;

namespace Planta.Api.Health;

public sealed class DbHealthCheck : IHealthCheck
{
    private readonly PlantaDbContext _db;

    public DbHealthCheck(PlantaDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Intenta conexión simple (no consulta tablas del negocio)
        var canConnect = await _db.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("Database: Healthy")
            : HealthCheckResult.Unhealthy("Database: Unreachable");
    }
}
