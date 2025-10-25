// Ruta: /Planta.Application/Mapping/MappingProfile.cs | V1.2
using AutoMapper;

namespace Planta.Application.Mapping;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Perfil vacío por ahora.
        // Motivo: Aún no hay referencia disponible a entidades de dominio en Planta.Application.
        // Cuando se incorpore Planta.Domain (o proyecciones EF en Infrastructure/Data),
        // activamos aquí los mapeos Recibo -> ReciboListItemDto / ReciboDetailDto.

        // Ejemplo (cuando exista el tipo de entidad):
        // CreateMap<Planta.Domain.Operacion.Recibo, Planta.Contracts.Recibos.ReciboListItemDto>();
        // CreateMap<Planta.Domain.Operacion.Recibo, Planta.Contracts.Recibos.ReciboDetailDto>();
    }
}
