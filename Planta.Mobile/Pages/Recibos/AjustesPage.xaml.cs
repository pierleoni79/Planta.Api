//Ruta: /Planta.Mobile/Pages/Recibos/AjustesPage.xaml.cs | V1.0
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Planta.Mobile.Pages.Recibos;

public partial class AjustesPage : ContentPage
{
    public AjustesPage()
    {
        InitializeComponent();
        ApiUrlEntry.Text = Preferences.Get("ApiBaseUrl", "http://10.0.2.2:5122/");
    }

    private void OnGuardarClicked(object sender, EventArgs e)
    {
        var url = ApiUrlEntry.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(url))
        {
            Preferences.Set("ApiBaseUrl", url!);
            DisplayAlert("Listo", "URL guardada", "OK");
        }
    }
}