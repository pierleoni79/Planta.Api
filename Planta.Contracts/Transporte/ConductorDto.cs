// Ruta: /Planta.Contracts/Transporte/ConductorDto.cs | V1.1
#nullable enable
namespace Planta.Contracts.Transporte;

public sealed record ConductorDto(
    int Id,
    string Nombre,
    bool Activo
);
