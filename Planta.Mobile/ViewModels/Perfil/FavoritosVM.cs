// Ruta: /Planta.Mobile/ViewModels/Perfil/FavoritosVM.cs | V1.2-fix
using Planta.Contracts.Transporte;
using Planta.Mobile.Services.Api;
using Planta.Mobile.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

// Usa tu propio RelayCommand (evita la colisión con el Toolkit)
using RelayCommand = Planta.Mobile.ViewModels.Base.RelayCommand;

namespace Planta.Mobile.ViewModels;

public sealed class FavoritosVM : BaseViewModel
{
    private readonly IFavoritosApi _fav;
    public ObservableCollection<VehiculoDto> Chips { get; } = new();

    public ICommand LoadCmd { get; }

    public FavoritosVM(IFavoritosApi fav)
    {
        Title = "Favoritos";
        _fav = fav;
        LoadCmd = new RelayCommand(async () => await LoadAsync(), () => !IsBusy);
    }

    private async Task LoadAsync()
    {
        if (IsBusy) return; IsBusy = true;
        try
        {
            Chips.Clear();
            foreach (var v in await _fav.GetAsync() ?? new()) Chips.Add(v);
        }
        finally { IsBusy = false; }
    }
}
