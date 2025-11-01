#nullable enable
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Planta.Mobile.Services.Api;

namespace Planta.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        // API client base (ajusta implementación concreta)
        builder.Services.AddSingleton<IApiClient, ApiClient>();
        // Transporte API
        builder.Services.AddSingleton<TransporteApi>();

        return builder.Build();
    }
}
