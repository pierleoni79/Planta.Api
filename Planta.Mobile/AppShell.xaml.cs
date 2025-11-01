// Ruta: /Planta.Mobile/AppShell.xaml.cs | V1.1-fix (DI + modal al iniciar + routing)
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Planta.Mobile;

public partial class AppShell : Shell
{
    private readonly IServiceProvider _sp;
    private bool _startupModalShown;

    public AppShell(IServiceProvider sp)
    {
        InitializeComponent();
        _sp = sp;

        // Rutas
        Routing.RegisterRoute("transito/detalle", typeof(Pages.Transito.ReciboDetallePage));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_startupModalShown) return;
        _startupModalShown = true;

        // Mostrar la modal "Nuevo Recibo" al iniciar
        _ = MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var modal = _sp.GetRequiredService<Pages.Recibos.NuevoReciboModalPage>();

            var nav = Shell.Current?.Navigation ?? this.Navigation;
            if (nav is not null)
            {
                await nav.PushModalAsync(modal);

                if (modal.BindingContext is ViewModels.Recibos.NuevoReciboVM vm)
                    _ = vm.CargarFavoritosAsync(); // precarga sin bloquear UI
            }
        });
    }
}
