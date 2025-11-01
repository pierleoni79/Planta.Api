// Ruta: /Planta.Application/Transporte/Queries/ListarVehiculosFavoritos/ListarVehiculosFavoritosQuery.cs | V1.0
#nullable enable
using MediatR;
using Planta.Contracts.Transporte;

namespace Planta.Application.Transporte.Queries.ListarVehiculosFavoritos;

public sealed record ListarVehiculosFavoritosQuery(int EmpresaId, int Max = 8)
    : IRequest<IReadOnlyList<VehiculoFavoritoDto>>;
