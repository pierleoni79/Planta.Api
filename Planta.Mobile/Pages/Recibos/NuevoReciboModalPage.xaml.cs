// Ruta: /Planta.Mobile/Pages/Recibos/NuevoReciboModalPage.xaml.cs | V1.1
#nullable enable
using Microsoft.Maui.Controls.Xaml;
using Planta.Mobile.ViewModels.Recibos;

namespace Planta.Mobile.Pages.Recibos;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class NuevoReciboModalPage : ContentPage
{
    private readonly NuevoReciboVM _vm;

    public NuevoReciboModalPage(NuevoReciboVM vm)
    {
        InitializeComponent();             // <- se genera a partir del XAML
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CargarFavoritosAsync();
    }
}
