// Ruta: /Planta.Contracts/Config/PlantaDto.cs | V1.1
#nullable enable
namespace Planta.Contracts.Config;

public sealed record PlantaDto(
    int Id,
    int EmpresaId,
    string Nombre
);
