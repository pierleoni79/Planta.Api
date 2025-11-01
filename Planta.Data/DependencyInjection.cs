// Ruta: /Planta.Data/DependencyInjection.cs | V1.4 (fix: usings a Application + FQ en AddScoped)
#nullable enable
using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Domain/Application contracts existentes en tu solución
using Planta.Application.Recibos.Abstractions;      // IReciboReadStore
using Planta.Domain.Repositories;                  // IUnitOfWork, IReciboRepository

// Implementaciones concretas de esta capa
using Planta.Data.Repositories;                    // ReciboRepository, TransporteRepository
using Planta.Data.ReadStores;                      // ReciboReadStore, TransporteReadStore, TarifaReadStore

// ADO helpers (opcionales)
using Planta.Data.Abstractions;                    // ISqlConnectionFactory

namespace Planta.Data;

public static class DependencyInjection
{
    /// <summary>
    /// Registra la capa de datos.
    /// </summary>
    /// <param name="services">DI container.</param>
    /// <param name="sqlServerConnectionString">Connection string para SQL Server.</param>
    /// <param name="registerAdoComponents">
    /// true: registra además los ReadStores/Repos ADO (Transporte/Tarifa) y la SqlConnectionFactory.
    /// false: solo EF (DbContext + Recibo repo/readstore).
    /// </param>
    public static IServiceCollection AddPlantaData(
        this IServiceCollection services,
        string sqlServerConnectionString,
        bool registerAdoComponents = true)
    {
        // ---------- EF Core: DbContext ----------
        services.AddDbContext<PlantaDbContext>(opt =>
        {
            opt.UseSqlServer(sqlServerConnectionString, sql =>
            {
                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });

            // Si la mayoría de lecturas son read-only, puedes considerar:
            // opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // ---------- UoW + Recibo (EF) ----------
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PlantaDbContext>());
        services.AddScoped<IReciboRepository, ReciboRepository>();
        services.AddScoped<IReciboReadStore, ReciboReadStore>();

        // ---------- (Opcional) ADO.NET components ----------
        if (registerAdoComponents)
        {
            // Factory de conexiones basada en string (evita acoplar a IConfiguration aquí)
            services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactoryFromString(sqlServerConnectionString));

            // ReadStores ADO (usa las interfaces que viven en Application)
            services.AddScoped<
                Planta.Application.Transporte.Abstractions.ITransporteReadStore,
                Planta.Data.ReadStores.TransporteReadStore>();

            services.AddScoped<
                Planta.Application.Tarifas.Abstractions.ITarifaReadStore,
                Planta.Data.ReadStores.TarifaReadStore>();

            // Repos ADO (mutaciones puntuales)
            services.AddScoped<
                Planta.Application.Transporte.Abstractions.ITransporteRepository,
                Planta.Data.Repositories.TransporteRepository>();
        }

        return services;
    }
}

/// <summary>
/// Factory simple que crea conexiones SqlConnection usando un connection string dado.
/// </summary>
internal sealed class SqlConnectionFactoryFromString : ISqlConnectionFactory
{
    private readonly string _cnn;

    public SqlConnectionFactoryFromString(string cnn)
        => _cnn = !string.IsNullOrWhiteSpace(cnn)
            ? cnn
            : throw new ArgumentNullException(nameof(cnn), "El connection string no puede ser null ni vacío.");

    public Microsoft.Data.SqlClient.SqlConnection Create()
        => new(_cnn);

    public async Task<Microsoft.Data.SqlClient.SqlConnection> CreateOpenAsync(CancellationToken ct = default)
    {
        var cn = new Microsoft.Data.SqlClient.SqlConnection(_cnn);
        await cn.OpenAsync(ct);
        return cn;
    }
}
