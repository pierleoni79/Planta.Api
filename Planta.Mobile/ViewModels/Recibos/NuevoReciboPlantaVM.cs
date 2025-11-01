// Ruta: /Planta.Mobile/ViewModels/Recibos/NuevoReciboPlantaVM.cs | V1.5-fix (POCO initializer)
#nullable enable
using System.Collections.ObjectModel;
using System.Windows.Input;
using Planta.Mobile.ViewModels.Base;
using Planta.Mobile.Models;                    // OptionItemInt
using Planta.Mobile.Models.Recibos;           // NuevoReciboForm
using Planta.Mobile.Services.Api;             // IRecibosApi, ICatalogosApi
using Planta.Mobile.Services;                 // AppState
using Planta.Contracts.Config;                // PlantaDto
using Planta.Contracts.Enums;                 // DestinoTipo
using Planta.Contracts.Recibos.Requests;      // ReciboCreateRequest, VincularOrigenRequest, RegistrarMaterialRequest

namespace Planta.Mobile.ViewModels;

public sealed class NuevoReciboPlantaVM : BaseViewModel
{
    private readonly IRecibosApi _recibos;
    private readonly ICatalogosApi _catalogos;
    private readonly AppState _state;

    public NuevoReciboForm Form { get; } = new();   // MaterialId = int

    public ObservableCollection<PlantaDto> Plantas { get; } = new();
    private PlantaDto? _selectedPlanta;
    public PlantaDto? SelectedPlanta
    {
        get => _selectedPlanta;
        set => SetProperty(ref _selectedPlanta, value);
    }

    public ObservableCollection<OptionItemInt> Materiales { get; } = new();
    private OptionItemInt? _selectedMaterial;
    public OptionItemInt? SelectedMaterial
    {
        get => _selectedMaterial;
        set
        {
            if (SetProperty(ref _selectedMaterial, value))
                Form.MaterialId = value?.Id ?? 0;
        }
    }

    public ICommand CrearCmd { get; }
    public ICommand LoadCmd { get; }

    public NuevoReciboPlantaVM(IRecibosApi recibos, ICatalogosApi catalogos, AppState state)
    {
        Title = "Nuevo (Planta)";
        _recibos = recibos;
        _catalogos = catalogos;
        _state = state;

        CrearCmd = new RelayCommand(async () => await CrearAsync(), () => !IsBusy);
        LoadCmd = new RelayCommand(async () => await LoadAsync(), () => !IsBusy);
    }

    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            Plantas.Clear();
            var pls = await _catalogos.GetPlantasAsync();
            if (pls is not null)
                foreach (var p in pls) Plantas.Add(p);

            Materiales.Clear();
            // TODO: reemplazar por catálogo real
            Materiales.Add(new OptionItemInt { Id = 10, Nombre = "Caliza (Proceso)" });
            Materiales.Add(new OptionItemInt { Id = 20, Nombre = "Subproducto" });
        }
        finally { IsBusy = false; }
    }

    private async Task CrearAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            if (SelectedPlanta is null) throw new InvalidOperationException("Planta es obligatoria.");
            if (Form.MaterialId <= 0) throw new InvalidOperationException("Material es obligatorio.");
            if (Form.Cantidad <= 0) throw new InvalidOperationException("Cantidad > 0.");

            var newId = Guid.NewGuid();
            var placaSnap = string.IsNullOrWhiteSpace(Form.Placa) ? null : Form.Placa!.Trim().ToUpperInvariant();
            var idem = string.IsNullOrWhiteSpace(Form.IdempotencyKey) ? newId.ToString("N") : Form.IdempotencyKey!;

            // 1) Crear (POCO → object initializer)
            var crearReq = new ReciboCreateRequest
            {
                Id = newId,
                EmpresaId = _state.EmpresaId,
                DestinoTipo = DestinoTipo.Planta,
                Cantidad = Form.Cantidad,
                IdempotencyKey = idem,
                PlacaSnapshot = placaSnap,
                ConductorNombreSnapshot = null,
                ReciboFisicoNumero = Form.ReciboFisicoNumero
            };

            var createdId = await _recibos.CrearAsync(crearReq);
            if (createdId is null) throw new InvalidOperationException("No se pudo crear el recibo.");

            // 2) Vincular origen (Planta)
            var okOrigen = await _recibos.VincularOrigenAsync(new VincularOrigenRequest(
                Id: createdId.Value,
                PlantaId: SelectedPlanta.Id,
                AlmacenOrigenId: null,
                ClienteId: null
            ));
            if (!okOrigen) throw new InvalidOperationException("No se pudo vincular la planta.");

            // 3) Registrar material
            var okMat = await _recibos.RegistrarMaterialAsync(new RegistrarMaterialRequest(
                Id: createdId.Value,
                MaterialId: Form.MaterialId,
                Cantidad: Form.Cantidad
            ));
            if (!okMat) throw new InvalidOperationException("No se pudo registrar el material.");
        }
        finally { IsBusy = false; }
    }
}
