#nullable enable
using Planta.Contracts.Transporte;

namespace Planta.Mobile.Services.Api;

public interface ITransporteApi
{
    Task<TransporteResolucionDto?> ResolverPorPlacaAsync(string placa, CancellationToken ct = default);
    Task<IReadOnlyList<VehiculoFavoritoDto>> ListarFavoritosAsync(int empresaId, int max, CancellationToken ct = default);
    Task<bool> ToggleFavoritoAsync(int vehiculoId, bool esFavorito, CancellationToken ct = default);
}
