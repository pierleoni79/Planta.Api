// Ruta: /Planta.Mobile/ViewModels/Recibos/NuevoReciboClienteVM.cs | V1.4-fix (POCO initializer)
#nullable enable
using System.Collections.ObjectModel;
using System.Windows.Input;
using Planta.Mobile.ViewModels.Base;
using Planta.Mobile.Models.Recibos;           // NuevoReciboForm
using Planta.Mobile.Services.Api;             // IRecibosApi, ICatalogosApi
using Planta.Mobile.Services;                 // AppState
using Planta.Contracts.CRM;                   // ClienteDto
using Planta.Contracts.Enums;                 // DestinoTipo
using Planta.Contracts.Recibos.Requests;      // ReciboCreateRequest, VincularOrigenRequest, RegistrarMaterialRequest

namespace Planta.Mobile.ViewModels;

public sealed class NuevoReciboClienteVM : BaseViewModel
{
    private readonly IRecibosApi _recibos;
    private readonly ICatalogosApi _catalogos;
    private readonly AppState _state;

    public NuevoReciboForm Form { get; } = new();
    public ObservableCollection<ClienteDto> Clientes { get; } = new();
    public ClienteDto? SelectedCliente { get; set; }
    public ObservableCollection<string> Unidades { get; } = new() { "m3", "ton" };

    public ICommand CrearCmd { get; }
    public ICommand LoadCmd { get; }

    public NuevoReciboClienteVM(IRecibosApi recibos, ICatalogosApi catalogos, AppState state)
    {
        Title = "Nuevo (Cliente)";
        _recibos = recibos; _catalogos = catalogos; _state = state;

        CrearCmd = new RelayCommand(async () => await CrearAsync(), () => !IsBusy);
        LoadCmd = new RelayCommand(async () => await LoadAsync(), () => !IsBusy);
    }

    private async Task LoadAsync()
    {
        if (IsBusy) return; IsBusy = true;
        try
        {
            Clientes.Clear();
            var list = await _catalogos.GetClientesAsync() ?? new();
            foreach (var c in list) Clientes.Add(c);
        }
        finally { IsBusy = false; }
    }

    private async Task CrearAsync()
    {
        if (IsBusy) return; IsBusy = true;
        try
        {
            if (SelectedCliente is null) throw new InvalidOperationException("Cliente es obligatorio.");
            if (Form.Cantidad <= 0) throw new InvalidOperationException("Cantidad > 0.");
            if (string.IsNullOrWhiteSpace(Form.Unidad)) throw new InvalidOperationException("Unidad es obligatoria.");

            var newId = Guid.NewGuid();
            var placaSnap = string.IsNullOrWhiteSpace(Form.Placa) ? null : Form.Placa!.Trim().ToUpperInvariant();
            var idem = string.IsNullOrWhiteSpace(Form.IdempotencyKey) ? newId.ToString("N") : Form.IdempotencyKey!;

            // 1) Crear: POCO ⇒ usar object initializer (no constructor con parámetros)
            var crearReq = new ReciboCreateRequest
            {
                Id = newId,
                EmpresaId = _state.EmpresaId,
                DestinoTipo = DestinoTipo.ClienteDirecto,
                Cantidad = Form.Cantidad,
                IdempotencyKey = idem,
                PlacaSnapshot = placaSnap,
                ConductorNombreSnapshot = null,
                ReciboFisicoNumero = Form.ReciboFisicoNumero
            };

            var createdId = await _recibos.CrearAsync(crearReq);
            if (createdId is null) throw new InvalidOperationException("No se pudo crear el recibo.");

            // 2) Vincular destino (Cliente)
            var okOrigen = await _recibos.VincularOrigenAsync(new VincularOrigenRequest(
                Id: createdId.Value,
                PlantaId: null,
                AlmacenOrigenId: null,
                ClienteId: SelectedCliente.Id
            ));
            if (!okOrigen) throw new InvalidOperationException("No se pudo vincular el cliente.");

            // 3) Registrar material (si aplica)
            if (Form.MaterialId is int mid && mid > 0)
            {
                var okMat = await _recibos.RegistrarMaterialAsync(new RegistrarMaterialRequest(
                    Id: createdId.Value,
                    MaterialId: mid,
                    Cantidad: Form.Cantidad
                ));
                if (!okMat) throw new InvalidOperationException("No se pudo registrar el material.");
            }

            // TODO: feedback de éxito / limpiar formulario / navegar
        }
        finally { IsBusy = false; }
    }
}
