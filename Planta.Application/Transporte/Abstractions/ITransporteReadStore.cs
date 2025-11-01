// Ruta: /Planta.Application/Transporte/Abstractions/ITransporteReadStore.cs | V1.0
#nullable enable
using Planta.Contracts.Transporte;

namespace Planta.Application.Transporte.Abstractions;

public interface ITransporteReadStore
{
    Task<IReadOnlyList<VehiculoFavoritoDto>> ListarFavoritosAsync(int empresaId, int max, CancellationToken ct);
    Task<TransporteResolucionDto?> ResolverTransportePorPlacaAsync(string placa, CancellationToken ct);
}
