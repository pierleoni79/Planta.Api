// Ruta: /Planta.Application/Transporte/Commands/ToggleVehiculoFavorito/ToggleVehiculoFavoritoHandler.cs | V1.2
#nullable enable
using MediatR;
using Planta.Application.Transporte.Abstractions;

namespace Planta.Application.Transporte.Commands.ToggleVehiculoFavorito;

public sealed class ToggleVehiculoFavoritoHandler : IRequestHandler<ToggleVehiculoFavoritoCommand, bool>
{
    private readonly ITransporteRepository _repo;
    public ToggleVehiculoFavoritoHandler(ITransporteRepository repo) => _repo = repo;

    public Task<bool> Handle(ToggleVehiculoFavoritoCommand request, CancellationToken ct)
        => _repo.ToggleFavoritoAsync(request.VehiculoId, request.EsFavorito, ct);
}
