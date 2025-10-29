// Ruta: /Planta.Mobile/Helpers/ServiceHelper.cs | V1.0
#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace Planta.Mobile
{
    public static class ServiceHelper
    {
        public static T GetRequiredService<T>() where T : notnull
            => Current.GetRequiredService<T>();

        public static IServiceProvider Current
            => Application.Current?.Handler?.MauiContext?.Services
               ?? throw new InvalidOperationException("ServiceProvider no disponible. Asegura inicialización de MAUI.");
    }
}
