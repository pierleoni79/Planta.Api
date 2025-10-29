// Ruta: /Planta.Mobile/App.xaml.cs | V1.1 (fix .NET 9: sin InitializeComponent, usa CreateWindow)
#nullable enable
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Planta.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            // Si realmente usas recursos en App.xaml y su Build Action = MauiXaml,
            // puedes descomentar la siguiente línea. Si no, déjala comentada.
            // InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Si prefieres resolver por DI:
            // var shell = ServiceHelper.GetRequiredService<AppShell>();
            var shell = new AppShell();
            return new Window(shell);
        }
    }
}
