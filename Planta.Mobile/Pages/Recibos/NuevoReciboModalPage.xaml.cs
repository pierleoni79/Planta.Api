// Ruta: /Planta.Mobile/Pages/Recibos/NuevoReciboModalPage.xaml.cs | V1.0
#nullable enable
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using Planta.Contracts.Transporte;
using Planta.Mobile.Popups; // Conductor/Planta/Material popups si decides usarlos
using Planta.Mobile.Services.Api; // ITransporteApi, IMaterialApi, IClienteApi, IPlantaApi

namespace Planta.Mobile.Pages.Recibos;

public sealed record NuevoReciboDraft(
    string Placa,
    int VehiculoId,
    int? ConductorId,
    int MaterialId,
    string Unidad,            // "m³" | "viaje"
    decimal Cantidad,
    string Destino,           // "Cliente" | "Planta"
    int? ClienteId,
    int? PlantaDestinoId,
    string? Observaciones
);

public partial class NuevoReciboModalPage : ContentPage
{
    private readonly ITransporteApi _transporteApi;
    private readonly IMaterialApi _materialApi;
    private readonly IClienteApi _clienteApi;
    private readonly IPlantaApi _plantaApi;

    private readonly TaskCompletionSource<NuevoReciboDraft?> _tcs = new();

    // Estado
    private TransporteResolucionDto? _vehiculo;
    private bool _unidadBloqueadaPorTarifa = false; // si hay tarifa, se bloquea
    private decimal _toleranciaPorc = 10m;          // ajustar según negocio/tarifa
    private decimal? _capacidadM3;                  // derivado de vehículo
    private string _plantaSesion = "—";
    private string _almacenDefecto = "—";

    // Listas para pickers
    private sealed record Item(int Id, string Nombre);
    private readonly ObservableCollection<Item> _materiales = new();
    private readonly ObservableCollection<Item> _clientes = new();
    private readonly ObservableCollection<Item> _plantas = new();

    public NuevoReciboModalPage(string? plantaSesionNombre = null, string? almacenDefectoNombre = null)
    {
        InitializeComponent();

        // DI
        var sp = Application.Current?.Handler?.MauiContext?.Services
                 ?? throw new InvalidOperationException("ServiceProvider no disponible.");
        _transporteApi = sp.GetRequiredService<ITransporteApi>();
        _materialApi = sp.GetRequiredService<IMaterialApi>();
        _clienteApi = sp.GetRequiredService<IClienteApi>();
        _plantaApi = sp.GetRequiredService<IPlantaApi>();

        // Sesión
        _plantaSesion = string.IsNullOrWhiteSpace(plantaSesionNombre) ? "Cantera" : plantaSesionNombre!;
        _almacenDefecto = string.IsNullOrWhiteSpace(almacenDefectoNombre) ? "Principal" : almacenDefectoNombre!;
        LblSubtitulo.Text = $"Origen: Cantera (planta de sesión)";
        LblAlmacen.Text = $"Almacén origen: Cantera – {_almacenDefecto}";

        // Picker data binding
        PickMaterial.ItemsSource = _materiales;
        PickCliente.ItemsSource = _clientes;
        PickPlanta.ItemsSource = _plantas;

        // Defaults
        SegDestino.SelectedIndex = 0; // Cliente por default (lo puedes cambiar)
        SegUnidad.SelectedIndex = 0;  // m³

        // Cargar catálogos básicos (sin bloquear UI)
        _ = LoadMaterialesAsync();
        _ = LoadClientesAsync();
        _ = LoadPlantasAsync();

        UpdateAyudaCantidad();
        RecomputeChecklistAndButton();
    }

    // ---- API carga catálogos (ajusta a tus endpoints reales) ----
    private async Task LoadMaterialesAsync()
    {
        try
        {
            var lista = await _materialApi.ListarAsync(); // ajusta: empresaId si aplica
            _materiales.Clear();
            foreach (var m in lista)
                _materiales.Add(new Item(m.Id, m.Nombre ?? $"ID {m.Id}"));
        }
        catch
        {
            // Si falla, deja el Picker vacío: el usuario puede usar tu popup buscador
        }
    }

    private async Task LoadClientesAsync()
    {
        try
        {
            var lista = await _clienteApi.ListarAsync(); // ajusta: top/empresaId
            _clientes.Clear();
            foreach (var c in lista)
                _clientes.Add(new Item(c.Id, c.Nombre ?? $"ID {c.Id}"));
        }
        catch { }
    }

    private async Task LoadPlantasAsync()
    {
        try
        {
            var lista = await _plantaApi.ListarAsync();
            _plantas.Clear();
            foreach (var p in lista)
                _plantas.Add(new Item(p.Id, p.Nombre ?? $"ID {p.Id}"));
        }
        catch { }
    }

    // ---- Interacción ----
    public static async Task<NuevoReciboDraft?> ShowAsync(INavigation nav, string? plantaSesionNombre = null, string? almacenDefectoNombre = null)
    {
        var page = new NuevoReciboModalPage(plantaSesionNombre, almacenDefectoNombre);
        await nav.PushModalAsync(new NavigationPage(page));
        return await page._tcs.Task;
    }

    private async void OnResolverPlacaClicked(object? sender, EventArgs e)
    {
        ErrPlaca.Text = "";
        var placa = (TxtPlaca.Text ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(placa))
        {
            ErrPlaca.Text = "Placa requerida.";
            return;
        }

        try
        {
            var dto = await _transporteApi.ResolverPorPlacaAsync(placa);
            if (dto is null)
            {
                ErrPlaca.Text = "No encontramos un vehículo con esta placa.";
                _vehiculo = null;
                LblVehiculoResumen.Text = "Vehículo: —";
                LblVehiculoClaseCap.Text = "Clase: — · Capacidad: — m³";
                LblConductor.Text = "Conductor: —";
                LblConductorFuente.Text = "—";
                AlertaConductor.Text = "";
                _capacidadM3 = null;
                UpdateAyudaCantidad();
                RecomputeChecklistAndButton();
                return;
            }

            _vehiculo = dto;
            _capacidadM3 = dto.CapacidadM3;
            LblVehiculoResumen.Text = dto.VehiculoActivo ? $"Vehículo ACTIVO • {dto.Placa}" : $"Vehículo INACTIVO • {dto.Placa}";
            if (!dto.VehiculoActivo) ErrPlaca.Text = "Vehículo inactivo. Verifica o selecciona otro.";

            LblVehiculoClaseCap.Text = $"Clase: {dto.ClaseNombre ?? "—"} · Capacidad: {(dto.CapacidadM3?.ToString("0.###") ?? "—")} m³";
            LblConductor.Text = $"Conductor: {dto.ConductorNombreSnapshot ?? "—"}";
            LblConductorFuente.Text = dto.FuenteResolucion switch
            {
                "Historial" => "Resuelto por historial vigente.",
                "Recibo" => "Sugerido por último recibo.",
                "Staging" => "Sugerido por staging.",
                _ => "—"
            };
            AlertaConductor.Text = dto.ConductorActivo == false ? "Conductor inactivo." : "";

            UpdateAyudaCantidad();
            RecomputeChecklistAndButton();
        }
        catch (Exception ex)
        {
            ErrPlaca.Text = "Error al resolver placa: " + ex.Message;
        }
    }

    private void OnPlacaCompleted(object? sender, EventArgs e) => OnResolverPlacaClicked(sender, e);

    private void OnMaterialChanged(object? sender, EventArgs e)
    {
        ErrMaterial.Text = "";
        RecomputeChecklistAndButton();
    }

    private void OnDestinoChanged(object? sender, SegmentedControlSelectionChangedEventArgs e)
    {
        var dest = GetDestino();
        SecCliente.IsVisible = dest == "Cliente";
        SecPlanta.IsVisible = dest == "Planta";
        ErrCliente.Text = "";
        ErrPlantaDestino.Text = "";
        RecomputeChecklistAndButton();
    }

    private void OnUnidadChanged(object? sender, SegmentedControlSelectionChangedEventArgs e)
    {
        if (_unidadBloqueadaPorTarifa)
        {
            // Volver a la unidad impuesta si alguien intenta cambiarla
            SegUnidad.SelectionChanged -= OnUnidadChanged;
            SegUnidad.SelectedIndex = GetUnidad() == "m³" ? 0 : 1;
            SegUnidad.SelectionChanged += OnUnidadChanged;
            ErrUnidad.Text = "Unidad fijada por tarifa.";
        }
        else
        {
            ErrUnidad.Text = "";
        }
        UpdateAyudaCantidad();
        RecomputeChecklistAndButton();
    }

    private void OnCantidadChanged(object? sender, TextChangedEventArgs e)
    {
        ErrCantidad.Text = "";
        UpdateAyudaCantidad();
        RecomputeChecklistAndButton();
    }

    private async void OnCambiarConductor(object? sender, EventArgs e)
    {
        // Puedes usar tu popup de conductor
        var dto = await ConductorResolverPopup.ShowAsync(null);
        if (dto is null) return;

        LblConductor.Text = $"Conductor: {dto.Nombre ?? "—"}";
        AlertaConductor.Text = dto.Activo ? "" : "Conductor inactivo.";
        // Nota: no cambiamos _vehiculo.ConductorId (es de solo lectura). Lo resuelves al construir el draft si necesitas override.
        RecomputeChecklistAndButton();
    }

    // ---- Helpers de estado/validación ----
    private string GetDestino() => SegDestino.SelectedIndex == 0 ? "Cliente" : "Planta";
    private string GetUnidad() => SegUnidad.SelectedIndex == 0 ? "m³" : "viaje";

    private decimal ParseCantidadOrZero()
    {
        var raw = (TxtCantidad.Text ?? "").Trim();
        if (string.IsNullOrEmpty(raw)) return 0m;
        // admitir coma o punto
        raw = raw.Replace(',', '.');
        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0m;
    }

    private void UpdateAyudaCantidad()
    {
        var unidad = GetUnidad();
        if (unidad == "m³")
        {
            if (_capacidadM3 is decimal cap)
            {
                var tol = _toleranciaPorc;
                var max = Math.Round(cap * (1m + tol / 100m), 3);
                LblAyudaCantidad.Text = $"Capacidad del vehículo: {cap:0.###} m³. Tolerancia: +{tol:0.#}% (máx. {max:0.###}).";
            }
            else
            {
                LblAyudaCantidad.Text = "Unidad = m³.";
            }
        }
        else
        {
            LblAyudaCantidad.Text = "Unidad de cobro por viaje. Usualmente 1 viaje.";
        }

        // Origen de la regla (unidad)
        LblOrigenUnidad.Text = _unidadBloqueadaPorTarifa ? "Unidad fijada por tarifa." : "Sin tarifa: captura manual.";
    }

    private bool Validate(out string checklistText)
    {
        bool ok = true;

        // Conectividad
        var online = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        // Placa
        if (_vehiculo is null)
        {
            ErrPlaca.Text = string.IsNullOrWhiteSpace(TxtPlaca.Text) ? "Placa requerida." : ErrPlaca.Text;
            ok = false;
        }
        else if (!_vehiculo.VehiculoActivo)
        {
            ok = false;
        }

        // Material
        var mat = PickMaterial.SelectedItem as Item;
        if (mat is null) { ErrMaterial.Text = "Material requerido."; ok = false; }

        // Destino
        if (GetDestino() == "Cliente")
        {
            if (PickCliente.SelectedItem is not Item) { ErrCliente.Text = "Cliente requerido."; ok = false; }
        }
        else
        {
            if (PickPlanta.SelectedItem is not Item) { ErrPlantaDestino.Text = "Planta destino requerida."; ok = false; }
        }

        // Unidad
        var unidad = GetUnidad();
        if (unidad != "m³" && unidad != "viaje") { ErrUnidad.Text = "Unidad requerida."; ok = false; }

        // Cantidad
        var cant = ParseCantidadOrZero();
        if (cant <= 0m) { ErrCantidad.Text = "Cantidad debe ser mayor que 0."; ok = false; }
        if (unidad == "m³" && _capacidadM3 is decimal cap)
        {
            var max = cap * (1m + _toleranciaPorc / 100m);
            if (cant > max + 0.0005m)
            {
                ErrCantidad.Text = "Cantidad supera la capacidad permitida del vehículo.";
                ok = false;
            }
        }

        // Checklist de pendientes
        checklistText =
            $"{(_vehiculo is not null && _vehiculo.VehiculoActivo ? "✓" : "•")} Vehículo encontrado\n" +
            $"{(!string.IsNullOrWhiteSpace(_vehiculo?.ConductorNombreSnapshot) ? "✓" : "•")} Conductor resuelto o confirmado\n" +
            $"{(mat is not null ? "✓" : "•")} Material seleccionado\n" +
            $"{(GetDestino() == \"Cliente\" ? (PickCliente.SelectedItem is not null ? "✓" : "•") : (PickPlanta.SelectedItem is not null ? "✓" : "•"))} Destino y entidad destino\n" +
            $"{( unidad is not null ? "✓" : "•")} Unidad definida\n" +
            $"{( cant > 0m ? "✓" : "•")} Cantidad > 0 y válida\n" +
            $"{( online ? "✓" : "•")} Conexión disponible";

        return ok && online;
    }

    private void RecomputeChecklistAndButton()
    {
        _ = Validate(out var text);
        LblChecklist.Text = text;
        BtnCrear.IsEnabled = Validate(out _);
    }

    private async void OnCrear(object? sender, EventArgs e)
    {
        if (!Validate(out var text))
        {
            LblChecklist.Text = text;
            return;
        }

        var mat = (Item)PickMaterial.SelectedItem!;
        var destino = GetDestino();
        int? clienteId = destino == "Cliente" ? (PickCliente.SelectedItem as Item)?.Id : null;
        int? plantaId = destino == "Planta" ? (PickPlanta.SelectedItem as Item)?.Id : null;

        var draft = new NuevoReciboDraft(
            Placa: _vehiculo!.Placa,
            VehiculoId: _vehiculo.VehiculoId,
            ConductorId: _vehiculo.ConductorId, // si cambiaste conductor con popup, sustitúyelo aquí
            MaterialId: mat.Id,
            Unidad: GetUnidad(),
            Cantidad: ParseCantidadOrZero(),
            Destino: destino,
            ClienteId: clienteId,
            PlantaDestinoId: plantaId,
            Observaciones: string.IsNullOrWhiteSpace(TxtObs.Text) ? null : TxtObs.Text.Trim()
        );

        _tcs.TrySetResult(draft);
        await Navigation.PopModalAsync();
    }

    // Cancelar (volver atrás) — puedes añadir un botón en la barra si lo prefieres
    protected override bool OnBackButtonPressed()
    {
        _tcs.TrySetResult(null);
        return base.OnBackButtonPressed();
    }
}
