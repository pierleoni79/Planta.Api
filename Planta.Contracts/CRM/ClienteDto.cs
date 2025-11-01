// Ruta: /Planta.Contracts/CRM/ClienteDto.cs | V1.1
#nullable enable
namespace Planta.Contracts.CRM;

public sealed record ClienteDto(
    int Id,
    string Nombre,
    bool Activo
);
