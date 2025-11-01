// Ruta: /Planta.Mobile/Pages/Settings/ConfiguracionPage.xaml.cs | V1.0
namespace Planta.Mobile.Pages.Settings;

public partial class ConfiguracionPage : ContentPage
{
    public ConfiguracionPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<Planta.Mobile.ViewModels.ConfiguracionVM>();
    }
}
