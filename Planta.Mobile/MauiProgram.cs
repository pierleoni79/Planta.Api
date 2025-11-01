// Ruta: /Planta.Mobile/MauiProgram.cs | V1.8-fix (HttpClient "api" + DI consistente + slash + cert dev)
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using Planta.Mobile.Pages.Perfil;
using Planta.Mobile.Pages.Recibos;
using Planta.Mobile.Pages.Settings;
using Planta.Mobile.Pages.Transito;
using Planta.Mobile.Services;
using Planta.Mobile.Services.Api;
using Planta.Mobile.ViewModels;
using Planta.Mobile.ViewModels.Recibos;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Planta.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();

        builder.ConfigureFonts(fonts =>
        {
            // fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // 1) Config base (emulador) + normalización URL
        var cfg = LoadAppConfig() ?? new AppConfig { BaseUrl = "http://10.0.2.2:5122" };
        cfg.BaseUrl = EnsureTrailingSlash(CanonicalizeForEmulator(cfg.BaseUrl)); // localhost → 10.0.2.2 + '/' final
        builder.Services.AddSingleton(cfg);

        // 2) HttpClient NOMBRE "api" (lo usará ApiClient internamente)
        builder.Services.AddHttpClient("api", client =>
        {
            client.BaseAddress = new Uri(cfg.BaseUrl!);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(30);
        })
#if DEBUG
        .ConfigurePrimaryHttpMessageHandler(() => BuildDevHandler(cfg.BaseUrl!))
#endif
        ;

        // 3) Estado de la App
        builder.Services.AddSingleton<AppState>();

        // 4) Infra / API wrappers
        builder.Services.AddSingleton<Services.Cache.IETagCacheService, Services.Cache.ETagCacheService>();
        builder.Services.AddSingleton<IApiClient, ApiClient>();     // ← usa HttpClient "api" por IHttpClientFactory
        builder.Services.AddSingleton<IRecibosApi, RecibosApi>();
        builder.Services.AddSingleton<IVehiculosApi, VehiculosApi>();
        builder.Services.AddSingleton<IConductoresApi, ConductoresApi>();
        builder.Services.AddSingleton<IFavoritosApi, FavoritosApi>();
        builder.Services.AddSingleton<ICatalogosApi, CatalogosApi>();

        // ⚠️ IMPORTANTE: estos dos servicios esperan IApiClient, NO HttpClient.
        builder.Services.AddSingleton<TransporteApi>();
        builder.Services.AddSingleton<TarifasApi>();

        // 5) ViewModels
        builder.Services.AddTransient<NuevoReciboClienteVM>();
        builder.Services.AddTransient<NuevoReciboPlantaVM>();
        builder.Services.AddTransient<TransitoVM>();
        builder.Services.AddTransient<ReciboDetalleVM>();
        builder.Services.AddTransient<FavoritosVM>();
        builder.Services.AddTransient<ConfiguracionVM>();
        builder.Services.AddTransient<NuevoReciboVM>();            // VM del modal unificado

        // 6) Pages
        builder.Services.AddTransient<NuevoReciboClientePage>();
        builder.Services.AddTransient<NuevoReciboPlantaPage>();
        builder.Services.AddTransient<TransitoPage>();
        builder.Services.AddTransient<ReciboDetallePage>();
        builder.Services.AddTransient<FavoritosPage>();
        builder.Services.AddTransient<ConfiguracionPage>();
        builder.Services.AddTransient<NuevoReciboModalPage>();     // página modal

        var app = builder.Build();

        // 7) Propaga BaseUrl a ApiClient (para que Configuración la modifique en runtime si hace falta)
        var api = app.Services.GetRequiredService<IApiClient>();
        if (!string.IsNullOrWhiteSpace(cfg.BaseUrl))
            api.BaseUrl = cfg.BaseUrl!;

        return app;
    }

    private static AppConfig? LoadAppConfig()
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.mobile.json").GetAwaiter().GetResult();
            using var sr = new StreamReader(stream);
            var json = sr.ReadToEnd();
            return JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }

    // Normaliza URLs de dev para el emulador Android
    private static string CanonicalizeForEmulator(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "http://10.0.2.2:5122";
        if (!Uri.TryCreate(raw, UriKind.Absolute, out var u)) return "http://10.0.2.2:5122";

        var host = u.Host.ToLowerInvariant();
        if (host is "localhost" or "127.0.0.1")
        {
            // Fuerza HTTP y mapea a 10.0.2.2 manteniendo el puerto
            var port = u.IsDefaultPort ? 80 : u.Port;
            return $"http://10.0.2.2:{port}";
        }
        return raw;
    }

    private static string EnsureTrailingSlash(string url)
        => url.EndsWith("/") ? url : (url + "/");

#if DEBUG
    // En DEBUG, si usas HTTPS a 10.0.2.2/localhost, acepta cert dev para evitar SSLHandshake
    private static HttpMessageHandler BuildDevHandler(string baseUrl)
    {
        var handler = new HttpClientHandler();
        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            var isHttps = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
            if (isHttps && (uri.Host == "10.0.2.2" || uri.Host == "localhost"))
            {
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }
        }
        return handler;
    }
#endif
}

public sealed class AppConfig
{
    public string? BaseUrl { get; set; }
}
