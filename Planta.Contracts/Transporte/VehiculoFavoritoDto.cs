// Ruta: /Planta.Contracts/Transporte/VehiculoFavoritoDto.cs | V1.0
#nullable enable
namespace Planta.Contracts.Transporte;

public sealed record VehiculoFavoritoDto(
    int VehiculoId,
    string Placa,
    string? ConductorNombre,
    DateTimeOffset? UltimoUso,
    bool EsFavorito
);
