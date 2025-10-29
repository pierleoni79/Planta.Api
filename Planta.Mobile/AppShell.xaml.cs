#nullable enable
using Microsoft.Maui.Controls;

namespace Planta.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // No necesitas Registrar rutas manualmente porque usas ShellContent con DataTemplate.
            // Si quisieras rutas adicionales sin ShellContent, podrías:
            // Routing.RegisterRoute("otra/ruta", typeof(Pages.Recibos.ReciboDetallePage));
        }
    }
}
