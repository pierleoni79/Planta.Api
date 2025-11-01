// Ruta: /Planta.Mobile/ViewModels/Transito/ReciboDetalleVM.cs | V1.1-fix
using Planta.Mobile.ViewModels.Base;
using Planta.Mobile.Services.Api;
using Planta.Contracts.Recibos;

namespace Planta.Mobile.ViewModels;

public sealed class ReciboDetalleVM : BaseViewModel
{
    private readonly IRecibosApi _recibos;
    public ReciboDto? Recibo { get; private set; }

    public ReciboDetalleVM(IRecibosApi recibos)
    {
        Title = "Detalle";
        _recibos = recibos;
    }

    public async Task LoadAsync(string id)
    {
        if (Guid.TryParse(id, out var gid))
        {
            Recibo = await _recibos.ObtenerAsync(gid);
            OnPropertyChanged(nameof(Recibo));
        }
    }
}
