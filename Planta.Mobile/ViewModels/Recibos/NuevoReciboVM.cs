// Ruta: /Planta.Mobile/ViewModels/Recibos/NuevoReciboVM.cs | V1.11-fix (item command + Appearing cmd)
#nullable enable
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Planta.Contracts.Enums;
using Planta.Contracts.Transporte;
using Planta.Mobile.Models.Recibos;
using Planta.Mobile.Services.Api;

namespace Planta.Mobile.ViewModels.Recibos;

public sealed class NuevoReciboVM : INotifyPropertyChanged
{
    // Ventana actual (MAUI single-window)
    private static Page? CurrentPage => Application.Current?.Windows.FirstOrDefault()?.Page;

    private readonly TransporteApi _tApi;
    private readonly TarifasApi _tfApi;
    private readonly IRecibosApi _recApi;

    public NuevoReciboVM(TransporteApi tApi, TarifasApi tfApi, IRecibosApi recApi)
    {
        _tApi = tApi;
        _tfApi = tfApi;
        _recApi = recApi;

        ResolverPlacaCommand = new Command(async () => await ResolverPlacaAsync());
        CalcularTarifaCommand = new Command(async () => await CalcularTarifaAsync());
        SeleccionarFavoritoCommand = new Command<VehiculoFavoritoVM>(OnSeleccionarFavorito);
        CrearCommand = new Command(async () => await CrearAsync(), () => !IsBusy);
        CerrarCommand = new Command(async () => await CerrarAsync());

        // Para disparar desde XAML en Appearing
        CargarFavoritosCommand = new Command(async () => await CargarFavoritosAsync());
    }

    // ---------- Estado ----------
    public ObservableCollection<VehiculoFavoritoVM> Favoritos { get; } = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set { _isBusy = value; OnChanged(); ((Command)CrearCommand).ChangeCanExecute(); }
    }

    public int EmpresaId { get; set; } = 1;

    private string? _placa;
    public string? Placa
    {
        get => _placa;
        set { _placa = value?.Trim().ToUpperInvariant(); OnChanged(); OnChanged(nameof(PlacaInfo)); }
    }

    public int? VehiculoId { get; private set; }
    public bool VehiculoActivo { get; private set; }
    public int? ClaseVehiculoId { get; private set; }
    public string? ClaseNombre { get; private set; }
    public decimal? CapacidadM3 { get; private set; }

    public int? ConductorId { get; private set; }
    private string? _conductorNombre;
    public string? ConductorNombre { get => _conductorNombre; set { _conductorNombre = value; OnChanged(); } }

    public string PlacaInfo =>
        VehiculoId is null
            ? "Resuelve la placa para precargar datos."
            : $"Vehículo #{VehiculoId} • Activo: {VehiculoActivo} • Clase: {ClaseNombre ?? "-"}";

    private int _destinoIndex; // 0=Planta, 1=Cliente
    public int DestinoIndex
    {
        get => _destinoIndex;
        set { _destinoIndex = value; OnChanged(); OnChanged(nameof(EsDestinoPlanta)); OnChanged(nameof(EsDestinoCliente)); }
    }
    public bool EsDestinoPlanta => DestinoIndex == 0;
    public bool EsDestinoCliente => DestinoIndex == 1;
    public DestinoTipo Destino => EsDestinoCliente ? DestinoTipo.ClienteDirecto : DestinoTipo.Planta;

    private int _materialId;
    public int MaterialId { get => _materialId; set { _materialId = value; OnChanged(); } }

    private int? _clienteId;
    public int? ClienteId { get => _clienteId; set { _clienteId = value; OnChanged(); } }

    private int? _plantaId;
    public int? PlantaId { get => _plantaId; set { _plantaId = value; OnChanged(); } }

    private string? _unidad;
    public string? Unidad
    {
        get => _unidad;
        set
        {
            _unidad = value;
            OnChanged();
            OnChanged(nameof(UnidadBloqueada));
            OnChanged(nameof(UnidadHabilitada));
        }
    }

    private bool _unidadIsLocked;
    public bool UnidadBloqueada => !string.IsNullOrWhiteSpace(Unidad) && _unidadIsLocked;
    public bool UnidadHabilitada => !UnidadBloqueada;

    private decimal _cantidad;
    public decimal Cantidad { get => _cantidad; set { _cantidad = value; OnChanged(); } }

    public int? TarifaId { get; private set; }
    public decimal? TarifaPrecio { get; private set; }
    public int TarifaPrioridad { get; private set; } = int.MaxValue;
    public string TarifaInfo =>
        TarifaId is null
            ? "Tarifa: (no encontrada)"
            : $"Tarifa #{TarifaId} • {Unidad ?? "-"} • Precio: {TarifaPrecio:C} • Prio: {TarifaPrioridad}";

    // ---------- Comandos ----------
    public ICommand CargarFavoritosCommand { get; }
    public ICommand ResolverPlacaCommand { get; }
    public ICommand CalcularTarifaCommand { get; }
    public ICommand SeleccionarFavoritoCommand { get; }
    public ICommand CrearCommand { get; }
    public ICommand CerrarCommand { get; }

    // ---------- Métodos ----------
    public async Task CargarFavoritosAsync()
    {
        try
        {
            Favoritos.Clear();
            var list = await _tApi.ListarFavoritosAsync(EmpresaId, max: 8, CancellationToken.None);
            foreach (var f in list)
            {
                var item = new VehiculoFavoritoVM(f, _tApi);
                item.SetSeleccionarHandler(OnSeleccionarFavorito); // wire-up del comando del item
                Favoritos.Add(item);
            }
        }
        catch (Exception ex)
        {
            await Alert($"No se pudieron cargar favoritos.\n{ex.Message}");
        }
    }

    private async Task ResolverPlacaAsync()
    {
        if (string.IsNullOrWhiteSpace(Placa)) { await Alert("Ingresa una placa."); return; }

        try
        {
            IsBusy = true;

            var dto = await _tApi.ResolverByPlacaAsync(Placa!, CancellationToken.None);
            if (dto is null)
            {
                LimpiarTransporte();
                await Alert($"No existe un vehículo con placa '{Placa}'.");
                return;
            }

            VehiculoId = dto.VehiculoId;
            ClaseVehiculoId = dto.ClaseVehiculoId;
            ClaseNombre = dto.ClaseNombre;
            CapacidadM3 = dto.CapacidadM3;
            VehiculoActivo = dto.VehiculoActivo;
            ConductorId = dto.ConductorId;
            ConductorNombre = dto.ConductorNombreSnapshot;

            OnChanged(nameof(ClaseNombre));
            OnChanged(nameof(CapacidadM3));
            OnChanged(nameof(VehiculoActivo));
            OnChanged(nameof(ConductorNombre));
            OnChanged(nameof(PlacaInfo));
        }
        catch (Exception ex)
        {
            await Alert($"Error resolviendo placa.\n{ex.Message}");
        }
        finally { IsBusy = false; }
    }

    private async Task CalcularTarifaAsync()
    {
        var err = ValidarAntesDeTarifa();
        if (err is not null) { await Alert(err); return; }

        try
        {
            IsBusy = true;

            var t = await _tfApi.ObtenerVigenteAsync(
                ClaseVehiculoId!.Value, MaterialId, Destino, ClienteId, PlantaId, CancellationToken.None);

            TarifaId = t.TarifaId;
            TarifaPrecio = t.Precio;
            TarifaPrioridad = t.Prioridad;
            Unidad = t.Unidad;
            _unidadIsLocked = !string.IsNullOrWhiteSpace(t.Unidad);

            if (EsUnidadM3(Unidad) && CapacidadM3 is not null && Cantidad <= 0)
            {
                Cantidad = CapacidadM3.Value; // autollenar por capacidad
                OnChanged(nameof(Cantidad));
            }

            OnChanged(nameof(Unidad));
            OnChanged(nameof(UnidadBloqueada));
            OnChanged(nameof(UnidadHabilitada));
            OnChanged(nameof(TarifaId));
            OnChanged(nameof(TarifaPrecio));
            OnChanged(nameof(TarifaPrioridad));
            OnChanged(nameof(TarifaInfo));
        }
        catch (Exception ex)
        {
            await Alert($"No fue posible obtener la tarifa.\n{ex.Message}");
        }
        finally { IsBusy = false; }
    }

    private async Task CrearAsync()
    {
        var err = ValidarAntesDeCrear();
        if (err is not null) { await Alert(err); return; }

        try
        {
            IsBusy = true;

            var form = new NuevoReciboForm
            {
                EmpresaId = EmpresaId,
                VehiculoId = VehiculoId,
                Placa = Placa,
                ConductorId = ConductorId,
                ConductorNombreSnapshot = ConductorNombre,
                Destino = Destino,
                ClienteId = ClienteId,
                PlantaId = PlantaId,
                MaterialId = MaterialId,
                Unidad = Unidad,
                Cantidad = Cantidad
            };

            var idem = Guid.NewGuid().ToString("N");

            // Firma: (form, idempotencyKey, ct)
            Guid id = await _recApi.CreateFullAsync(form, idem, CancellationToken.None);

            await Alert($"Recibo creado: {id}");
            await CerrarAsync();
        }
        catch (Exception ex)
        {
            await Alert($"No se pudo crear el recibo.\n{ex.Message}");
        }
        finally { IsBusy = false; }
    }

    private async Task CerrarAsync()
    {
        if (CurrentPage is not null)
            await CurrentPage.Navigation.PopModalAsync();
    }

    private static bool EsUnidadM3(string? u)
        => !string.IsNullOrWhiteSpace(u) && u.Trim().Equals("m3", StringComparison.OrdinalIgnoreCase);

    private void LimpiarTransporte()
    {
        VehiculoId = null;
        ClaseVehiculoId = null;
        ClaseNombre = null;
        CapacidadM3 = null;
        ConductorId = null;
        ConductorNombre = null;
        VehiculoActivo = false;
        OnChanged(nameof(PlacaInfo));
        OnChanged(nameof(ClaseNombre));
        OnChanged(nameof(CapacidadM3));
        OnChanged(nameof(ConductorNombre));
        OnChanged(nameof(VehiculoActivo));
    }

    private string? ValidarAntesDeTarifa()
    {
        if (ClaseVehiculoId is null) return "Falta resolver la placa para conocer la clase de vehículo.";
        if (MaterialId <= 0) return "Debes indicar el MaterialId.";
        if (EsDestinoCliente && (ClienteId is null || ClienteId <= 0)) return "Para destino ClienteDirecto debes indicar ClienteId.";
        if (EsDestinoPlanta && (PlantaId is null || PlantaId <= 0)) return "Para destino Planta debes indicar PlantaId.";
        return null;
    }

    private string? ValidarAntesDeCrear()
    {
        if (string.IsNullOrWhiteSpace(Placa)) return "Debes ingresar la placa.";
        if (VehiculoId is null) return "Resuelve la placa antes de crear.";
        if (MaterialId <= 0) return "Debes indicar el MaterialId.";
        if (string.IsNullOrWhiteSpace(Unidad)) return "Debes indicar la Unidad.";
        if (Cantidad <= 0) return "La Cantidad debe ser mayor que cero.";
        if (EsUnidadM3(Unidad) && CapacidadM3 is not null && Cantidad > CapacidadM3.Value)
            return $"La Cantidad ({Cantidad:0.###} m³) excede la capacidad del vehículo ({CapacidadM3:0.###} m³).";
        if (EsDestinoCliente && (ClienteId is null || ClienteId <= 0)) return "Para destino ClienteDirecto debes indicar ClienteId.";
        if (EsDestinoPlanta && (PlantaId is null || PlantaId <= 0)) return "Para destino Planta debes indicar PlantaId.";
        return null;
    }

    // Handler: seleccionar favorito
    private void OnSeleccionarFavorito(VehiculoFavoritoVM? fav)
    {
        if (fav is null) return;
        Placa = fav.Placa;
        _ = ResolverPlacaAsync(); // precarga datos
    }

    private Task Alert(string msg) =>
        CurrentPage is null ? Task.CompletedTask : CurrentPage.DisplayAlert("Nuevo Recibo", msg, "OK");

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// ====== VM auxiliar para el banner de favoritos ======
public sealed class VehiculoFavoritoVM : INotifyPropertyChanged
{
    private readonly TransporteApi _tApi;

    public int VehiculoId { get; }
    public string Placa { get; }
    public string? ConductorNombre { get; }
    public DateTimeOffset? UltimoUso { get; }
    public bool EsFavorito { get; private set; }

    public string Estrella => EsFavorito ? "★" : "☆";

    public ICommand ToggleFavoritoCommand { get; }
    // Comando que "inyecta" el padre para seleccionar el chip
    public ICommand? SeleccionarFavoritoCommand { get; private set; }

    public VehiculoFavoritoVM(VehiculoFavoritoDto dto, TransporteApi tApi)
    {
        VehiculoId = dto.VehiculoId;
        Placa = dto.Placa;
        ConductorNombre = dto.ConductorNombre;
        UltimoUso = dto.UltimoUso;
        EsFavorito = dto.EsFavorito;
        _tApi = tApi;

        ToggleFavoritoCommand = new Command(async () => await ToggleAsync());
    }

    // Inyección del handler desde el VM padre
    public void SetSeleccionarHandler(Action<VehiculoFavoritoVM> onSelect)
    {
        SeleccionarFavoritoCommand = new Command(() => onSelect(this));
        OnChanged(nameof(SeleccionarFavoritoCommand));
    }

    private async Task ToggleAsync()
    {
        try
        {
            var ok = await _tApi.ToggleFavoritoAsync(VehiculoId, !EsFavorito, CancellationToken.None);
            if (ok)
            {
                EsFavorito = !EsFavorito;
                OnChanged(nameof(EsFavorito));
                OnChanged(nameof(Estrella));
            }
            else
            {
                await Alert("No se pudo actualizar el estado de favorito.");
            }
        }
        catch (Exception ex)
        {
            await Alert($"Error cambiando favorito.\n{ex.Message}");
        }
    }

    private static Page? CurrentPage => Application.Current?.Windows.FirstOrDefault()?.Page;
    private Task Alert(string msg) =>
        CurrentPage is null ? Task.CompletedTask : CurrentPage.DisplayAlert("Favoritos", msg, "OK");

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
