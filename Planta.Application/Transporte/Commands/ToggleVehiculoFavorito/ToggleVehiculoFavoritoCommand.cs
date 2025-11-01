// Ruta: /Planta.Application/Transporte/Commands/ToggleVehiculoFavorito/ToggleVehiculoFavoritoCommand.cs | V1.0
#nullable enable
using MediatR;

namespace Planta.Application.Transporte.Commands.ToggleVehiculoFavorito;

public sealed record ToggleVehiculoFavoritoCommand(int VehiculoId, bool EsFavorito) : IRequest<bool>;
