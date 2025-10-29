// Ruta: /Planta.Data/Mapping/RecibosProfile.cs | V1.2
#nullable enable
using AutoMapper;
using Planta.Contracts.Recibos;
using ReciboEntity = Planta.Data.Entities.Recibo;

namespace Planta.Data.Mapping
{
    /// <summary>
    /// Mapeos Entity → Contracts para Recibos.
    /// </summary>
    public sealed class RecibosProfile : Profile
    {
        public RecibosProfile()
        {
            // Listado
            CreateMap<ReciboEntity, ReciboListItemDto>()
                .ForMember(d => d.Placa, m => m.MapFrom(s => s.PlacaSnapshot))
                .ForMember(d => d.Conductor, m => m.MapFrom(s => s.ConductorNombreSnapshot))
                .ForMember(d => d.Estado, m => m.MapFrom(s => (ReciboEstado)s.Estado));

            // Detalle
            CreateMap<ReciboEntity, ReciboDetailDto>()
                .ForMember(d => d.Placa, m => m.MapFrom(s => s.PlacaSnapshot))
                .ForMember(d => d.ConductorNombre, m => m.MapFrom(s => s.ConductorNombreSnapshot))
                .ForMember(d => d.Estado, m => m.MapFrom(s => (ReciboEstado)s.Estado));
        }
    }
}
