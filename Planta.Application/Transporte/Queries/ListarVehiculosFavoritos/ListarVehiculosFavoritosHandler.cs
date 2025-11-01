// Ruta: /Planta.Application/Transporte/Queries/ListarVehiculosFavoritos/ListarVehiculosFavoritosHandler.cs | V1.2-fix
#nullable enable
using MediatR;
using Planta.Contracts.Transporte;
using Planta.Application.Transporte.Abstractions;

namespace Planta.Application.Transporte.Queries.ListarVehiculosFavoritos;

public sealed class ListarVehiculosFavoritosHandler(ITransporteReadStore rs)
    : IRequestHandler<ListarVehiculosFavoritosQuery, IReadOnlyList<VehiculoFavoritoDto>>
{
    public Task<IReadOnlyList<VehiculoFavoritoDto>> Handle(ListarVehiculosFavoritosQuery request, CancellationToken ct)
        => rs.ListarFavoritosAsync(request.EmpresaId, request.Max, ct); // ← nombre correcto
}
