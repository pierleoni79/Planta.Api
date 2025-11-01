// Ruta: /Planta.Contracts/Config/AlmacenDto.cs | V1.1
#nullable enable
namespace Planta.Contracts.Config;

public sealed record AlmacenDto(
    int Id,
    int EmpresaId,
    int? PlantaId,
    string Nombre
);
