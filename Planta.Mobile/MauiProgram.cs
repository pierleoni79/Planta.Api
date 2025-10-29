// Ruta: /Planta.Mobile/MauiProgram.cs | V1.2 (completo)
#nullable enable
using CommunityToolkit.Maui;                 // <— importante para MCT001
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Planta.Mobile.Services;

namespace Planta.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()              // <— requiere que exista Planta.Mobile.App
                .UseMauiCommunityToolkit()      // <— inicializa .NET MAUI Community Toolkit
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ---------------- DI (Infraestructura) ----------------
            builder.Services.AddSingleton<IApiClientFactory, ApiClientFactory>();
            builder.Services.AddSingleton<IEtagStore, EtagStore>();
            builder.Services.AddSingleton<ICacheStore, JsonCacheStore>();

            // ---------------- DI (Servicios de dominio) -----------
            builder.Services.AddTransient<IApiRecibos, ApiRecibos>();
            builder.Services.AddTransient<IApiRecibosOperaciones, ApiRecibosOperaciones>();

            // ---------------- DI (ViewModels) ----------------------
            builder.Services.AddTransient<ViewModels.Recibos.RecibosListVm>();
            builder.Services.AddTransient<ViewModels.Recibos.ReciboProcesosVm>();

            // ---------------- DI (Pages) ---------------------------
            builder.Services.AddTransient<Pages.Recibos.RecibosListaPage>();
            builder.Services.AddTransient<Pages.Recibos.ReciboDetallePage>();
            builder.Services.AddTransient<Pages.Recibos.AjustesPage>(); // opcional

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
