// Ruta: /Planta.Mobile/ViewModels/Recibos/ReciboProcesosVm.cs | V1.4
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;                 // Windows.FirstOrDefault()
using System.Reflection;           // reflexión para ConductorDisplay
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;     // IQueryAttributable, Page
using Planta.Contracts.Recibos;
using Planta.Mobile.Services;
using Planta.Mobile;               // ServiceHelper

namespace Planta.Mobile.ViewModels.Recibos;

public sealed partial class ReciboProcesosVm : ObservableObject, IQueryAttributable
{
    private readonly IApiRecibos _api;
    private readonly IApiRecibosOperaciones _ops;
    private string? _etag;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private ReciboDetailDto? _recibo;

    // ---- Acciones (comandos) ----
    public IAsyncRelayCommand RefreshCmd { get; }
    public IAsyncRelayCommand CheckInCmd { get; }
    public IAsyncRelayCommand DescargaInicioCmd { get; }
    public IAsyncRelayCommand DescargaFinCmd { get; }

    // ---- Constructores ----
    // DI
    public ReciboProcesosVm(IApiRecibos api, IApiRecibosOperaciones ops)
    {
        _api = api;
        _ops = ops;
        RefreshCmd = new AsyncRelayCommand(RefreshAsync);
        CheckInCmd = new AsyncRelayCommand(() => RunAsync(r => _ops.EjecutarCheckInAsync(r.Id, _etag!, Guid.NewGuid(), CancellationToken.None)));
        DescargaInicioCmd = new AsyncRelayCommand(() => RunAsync(r => _ops.EjecutarDescargaInicioAsync(r.Id, _etag!, Guid.NewGuid(), CancellationToken.None)));
        DescargaFinCmd = new AsyncRelayCommand(() => RunAsync(r => _ops.EjecutarDescargaFinAsync(r.Id, _etag!, Guid.NewGuid(), CancellationToken.None)));
    }

    // Para XAML (resuelve dependencias vía ServiceProvider)
    public ReciboProcesosVm() : this(
        ServiceHelper.GetRequiredService<IApiRecibos>(),
        ServiceHelper.GetRequiredService<IApiRecibosOperaciones>())
    { }

    // ---- Navegación: recibe ?id=... ----
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && Guid.TryParse(idObj?.ToString(), out var id))
            _ = LoadAsync(id);
    }

    // ---- Carga / Refresh ----
    private async Task LoadAsync(Guid id)
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var (dto, etag) = await _api.ObtenerAsync(id, _etag, CancellationToken.None);
            Recibo = dto;
            _etag = etag;
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", ex.Message);
        }
        finally { IsBusy = false; }
    }

    private Task RefreshAsync()
        => Recibo is null ? Task.CompletedTask : LoadAsync(Recibo.Id);

    // ---- Ejecutar operación con If-Match + Idempotency ----
    private async Task RunAsync(Func<ReciboDetailDto, Task> op)
    {
        if (Recibo is null || string.IsNullOrWhiteSpace(_etag))
        {
            await ShowAlertAsync("Info", "Actualiza el recibo antes de operar.");
            return;
        }
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            await op(Recibo);
            await LoadAsync(Recibo.Id); // refrescar para obtener nuevo ETag/estado
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", ex.Message);
        }
        finally { IsBusy = false; }
    }

    // ---- Vista: propiedad robusta para mostrar el Conductor ----
    public string? ConductorDisplay => FirstNonEmpty(
        TryGetStringProp(Recibo, "ConductorNombre"),
        TryGetStringProp(Recibo, "Conductor"),
        TryGetStringProp(Recibo, "ConductorNombreSnapshot")
    );

    // Se dispara al cambiar Recibo gracias a [ObservableProperty]
    partial void OnReciboChanged(ReciboDetailDto? value)
        => OnPropertyChanged(nameof(ConductorDisplay));

    private static string? TryGetStringProp(object? obj, string propName)
    {
        if (obj is null) return null;
        var pi = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
        if (pi is null || pi.PropertyType != typeof(string)) return null;
        var val = (string?)pi.GetValue(obj);
        return string.IsNullOrWhiteSpace(val) ? null : val;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var v in values)
            if (!string.IsNullOrWhiteSpace(v))
                return v;
        return null;
    }

    // ---- Alertas (sin API obsoleta) ----
    private static Task ShowAlertAsync(string title, string message)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        return page is not null
            ? page.DisplayAlert(title, message, "OK")
            : Task.CompletedTask;
    }
}
