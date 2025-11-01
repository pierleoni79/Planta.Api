// Ruta: /Planta.Mobile/Pages/Recibos/NuevoReciboClientePage.xaml.cs | V1.0
using Microsoft.Maui.Controls;
using Planta.Mobile.ViewModels;

namespace Planta.Mobile.Pages.Recibos;

public partial class NuevoReciboClientePage : ContentPage
{
    public NuevoReciboClientePage()
    {
        InitializeComponent();

        var vm = ServiceHelper.GetService<NuevoReciboClienteVM>();
        BindingContext = vm;

        // Carga catálogos
        vm.LoadCmd.Execute(null);
    }
}
