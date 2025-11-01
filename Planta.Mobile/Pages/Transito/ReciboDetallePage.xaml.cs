// Ruta: /Planta.Mobile/Pages/Transito/ReciboDetallePage.xaml.cs | V1.0
using Microsoft.Maui.Controls;
using Planta.Mobile.ViewModels;

namespace Planta.Mobile.Pages.Transito;

[QueryProperty(nameof(ReciboId), "id")]
public partial class ReciboDetallePage : ContentPage
{
    public string? ReciboId { get; set; }

    public ReciboDetallePage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<ReciboDetalleVM>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrWhiteSpace(ReciboId))
        {
            var vm = (ReciboDetalleVM)BindingContext;
            await vm.LoadAsync(ReciboId);
        }
    }
}
