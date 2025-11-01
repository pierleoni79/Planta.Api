// Ruta: /Planta.Mobile/ServiceHelper.cs | V1.0
using Microsoft.Extensions.DependencyInjection;

namespace Planta.Mobile;

public static class ServiceHelper
{
    public static T GetService<T>() where T : notnull
        => Current.GetRequiredService<T>();

    public static IServiceProvider Current
        => Application.Current?.Handler?.MauiContext?.Services
           ?? throw new InvalidOperationException("No se pudo resolver IServiceProvider (MauiContext.Services).");
}
