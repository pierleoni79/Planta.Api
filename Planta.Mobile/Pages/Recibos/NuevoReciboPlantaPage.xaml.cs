// Ruta: /Planta.Mobile/Pages/Recibos/NuevoReciboPlantaPage.xaml.cs | V1.0
using Microsoft.Maui.Controls;
using Planta.Mobile.ViewModels;

namespace Planta.Mobile.Pages.Recibos;

public partial class NuevoReciboPlantaPage : ContentPage
{
    public NuevoReciboPlantaPage()
    {
        InitializeComponent();

        var vm = ServiceHelper.GetService<NuevoReciboPlantaVM>();
        BindingContext = vm;

        // Carga catálogos
        vm.LoadCmd.Execute(null);
    }
}
