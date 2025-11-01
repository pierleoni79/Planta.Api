// Ruta: /Planta.Contracts/Transporte/VehiculoDto.cs | V1.1
#nullable enable
namespace Planta.Contracts.Transporte;

public sealed record VehiculoDto(
    int Id,
    string Placa,
    int? ClaseVehiculoId,
    bool Activo
);
