// Ruta: /Planta.Mobile/ViewModels/Transito/TransitoVM.cs | V1.1-fix
using Planta.Mobile.ViewModels.Base;
using Planta.Mobile.Services.Api;
using Planta.Mobile.Services;
using Planta.Contracts.Recibos;
using System.Collections.ObjectModel;

namespace Planta.Mobile.ViewModels;

public sealed class TransitoVM : BaseViewModel
{
    private readonly IRecibosApi _recibos;
    private readonly AppState _state;

    public ObservableCollection<ReciboListItemDto> Items { get; } = new();

    public RelayCommand RefreshCmd { get; }

    public TransitoVM(IRecibosApi recibos, AppState state)
    {
        Title = "En tránsito";
        _recibos = recibos; _state = state;
        RefreshCmd = new RelayCommand(async () => await LoadAsync(), () => !IsBusy);
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return; IsBusy = true;
        try
        {
            Items.Clear();
            var page = await _recibos.ListarEnTransitoAsync(_state.EmpresaId);
            foreach (var it in page?.Items ?? Array.Empty<ReciboListItemDto>())
                Items.Add(it);
        }
        finally { IsBusy = false; }
    }
}
