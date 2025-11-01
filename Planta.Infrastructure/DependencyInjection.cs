// Ruta: /Planta.Infrastructure/DependencyInjection.cs | V1.2
#nullable enable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;

namespace Planta.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registra servicios de infraestructura (reloj, http, cache, etc.).</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Reloj del sistema (IClock)
        services.AddSingleton<IClock, SystemClock>();

        // HttpClient(s) tipados: descomenta y configura cuando los uses.
        // services.AddHttpClient<IProcesosHttpClient, ProcesosHttpClient>(client =>
        // {
        //     client.BaseAddress = new Uri(configuration["Services:Procesos:BaseUrl"]!);
        // });

        // Caché en memoria (si la necesitas)
        services.AddMemoryCache();

        return services;
    }
}
