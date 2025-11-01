// Ruta: /Planta.Mobile/Pages/Transito/TransitoPage.xaml.cs | V1.0
using Microsoft.Maui.Controls;
using Planta.Mobile.ViewModels;

namespace Planta.Mobile.Pages.Transito;

public partial class TransitoPage : ContentPage
{
    public TransitoPage()
    {
        InitializeComponent();

        var vm = ServiceHelper.GetService<TransitoVM>();
        BindingContext = vm;

        // Carga inicial
        _ = vm.LoadAsync();
    }
}
