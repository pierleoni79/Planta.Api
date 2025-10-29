// Ruta: /Planta.Mobile/ViewModels/Recibos/RecibosListVm.cs | V1.2
#nullable enable
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planta.Contracts.Recibos;
using Planta.Mobile.Services;
using Planta.Mobile; // ServiceHelper

namespace Planta.Mobile.ViewModels.Recibos
{
    public sealed partial class RecibosListVm : ObservableObject
    {
        private readonly IApiRecibos _api;

        [ObservableProperty] private bool _isBusy;
        [ObservableProperty] private string? _estadoFiltro;
        [ObservableProperty] private string? _search;

        public ObservableCollection<ReciboListItemDto> Items { get; } = new();

        public IAsyncRelayCommand RefreshCmd { get; }
        public IAsyncRelayCommand<ReciboListItemDto> OpenCmd { get; }

        // Ctor normal (DI)
        public RecibosListVm(IApiRecibos api)
        {
            _api = api;
            RefreshCmd = new AsyncRelayCommand(RefreshAsync);
            OpenCmd = new AsyncRelayCommand<ReciboListItemDto>(OpenAsync);
        }

        // Ctor requerido por XAML (resuelve via ServiceProvider)
        public RecibosListVm() : this(ServiceHelper.GetRequiredService<IApiRecibos>()) { }

        private async Task RefreshAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var (list, _) = await _api.ListarAsync(EstadoFiltro, Search, null, CancellationToken.None);
                Items.Clear();
                foreach (var it in list) Items.Add(it);
            }
            finally { IsBusy = false; }
        }

        private Task OpenAsync(ReciboListItemDto? dto)
        {
            if (dto is null) return Task.CompletedTask;
            return Shell.Current.GoToAsync($"recibo/detalle?id={dto.Id}");
        }
    }
}
