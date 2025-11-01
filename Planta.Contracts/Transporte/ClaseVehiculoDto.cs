// Ruta: /Planta.Contracts/Transporte/ClaseVehiculoDto.cs | V1.1
#nullable enable
namespace Planta.Contracts.Transporte;

public sealed record ClaseVehiculoDto(
    int Id,
    string Nombre,
    bool Activo
);
