#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Planta.Contracts.Transporte;
using Planta.Mobile.Services.Api; // ITransporteApi

namespace Planta.Mobile.Pages.Shared;

public partial class PlacaResolverModalPage : ContentPage
{
    private readonly ITransporteApi _transporte;
    private readonly TaskCompletionSource<TransporteResolucionDto?> _tcs = new();
    private TransporteResolucionDto? _dto;

    public PlacaResolverModalPage(string? placaInicial = null)
    {
        InitializeComponent();
        _transporte = Application.Current!.Services.GetRequiredService<ITransporteApi>();
        TxtPlaca.Text = placaInicial?.Trim().ToUpperInvariant();
    }

    // Uso: var dto = await PlacaResolverModalPage.ShowAsync(Navigation, placaActual);
    public static async Task<TransporteResolucionDto?> ShowAsync(INavigation nav, string? initialPlaca = null)
    {
        var page = new PlacaResolverModalPage(initialPlaca);
        await nav.PushModalAsync(new NavigationPage(page)); // modal con barra
        return await page._tcs.Task;
    }

    private async void OnResolverClicked(object sender, EventArgs e)
    {
        LblError.Text = "";
        BtnAceptar.IsEnabled = false;
        _dto = null;

        var placa = (TxtPlaca.Text ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(placa))
        {
            LblError.Text = "Placa requerida.";
            return;
        }

        try
        {
            var dto = await _transporte.ResolverPorPlacaAsync(placa);
            if (dto is null)
            {
                LblEstado.Text = "No encontrada";
                LblClase.Text = "Clase: —";
                LblCap.Text = "Capacidad: — m³";
                LblCond.Text = "Conductor: —";
                LblFuente.Text = "Fuente: —";
                LblError.Text = "No encontramos un vehículo con esta placa.";
                return;
            }

            _dto = dto;

            LblEstado.Text = dto.VehiculoActivo ? "Vehículo ACTIVO" : "Vehículo INACTIVO";
            LblClase.Text = $"Clase: {dto.ClaseNombre ?? "—"}";
            LblCap.Text = $"Capacidad: {(dto.CapacidadM3?.ToString("0.###") ?? "—")} m³";
            LblCond.Text = $"Conductor: {dto.ConductorNombreSnapshot ?? "—"}";
            LblFuente.Text = dto.FuenteResolucion switch
            {
                "Historial" => "Resuelto por historial vigente.",
                "Recibo" => "Sugerido por último recibo.",
                "Staging" => "Sugerido por staging.",
                _ => "—"
            };

            if (!dto.VehiculoActivo)
            {
                LblError.Text = "Vehículo inactivo. Verifica o selecciona otro.";
                BtnAceptar.IsEnabled = false;
            }
            else
            {
                BtnAceptar.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al resolver placa: " + ex.Message;
        }
    }

    private async void OnCancelar(object sender, EventArgs e)
    {
        _tcs.TrySetResult(null);
        await Navigation.PopModalAsync();
    }

    private async void OnAceptar(object sender, EventArgs e)
    {
        _tcs.TrySetResult(_dto);
        await Navigation.PopModalAsync();
    }
}
