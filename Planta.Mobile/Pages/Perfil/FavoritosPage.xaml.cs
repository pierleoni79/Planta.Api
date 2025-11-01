// Ruta: /Planta.Mobile/Pages/Perfil/FavoritosPage.xaml.cs | V1.0
using Microsoft.Maui.Controls;
using Planta.Mobile.ViewModels;

namespace Planta.Mobile.Pages.Perfil;

public partial class FavoritosPage : ContentPage
{
    public FavoritosPage()
    {
        InitializeComponent();

        var vm = ServiceHelper.GetService<FavoritosVM>();
        BindingContext = vm;

        vm.LoadCmd.Execute(null);
    }
}
