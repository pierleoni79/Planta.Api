// Ruta: /Planta.Data/Mapping/RecibosProfile.cs | V1.3
#nullable enable
using AutoMapper;
using Planta.Contracts.Recibos;
using ReciboEntity = Planta.Data.Entities.Recibo;

namespace Planta.Data.Mapping
{
    /// <summary>
    /// Mapeos Entity (op.Recibo) → Contracts.DTO para Recibos.
    /// - Estado (byte) se castea a ReciboEstado (enum de Contracts).
    /// - Placa/Conductor salen de los snapshots persistidos.
    /// </summary>
    public sealed class RecibosProfile : Profile
    {
        public RecibosProfile()
        {
            // === Listado ===
            CreateMap<ReciboEntity, ReciboListItemDto>()
                .ForMember(d => d.Placa, m => m.MapFrom(s => s.PlacaSnapshot))
                .ForMember(d => d.Conductor, m => m.MapFrom(s => s.ConductorNombreSnapshot))
                .ForMember(d => d.Estado, m => m.MapFrom(s => (ReciboEstado)s.Estado))
                // No sobreescribir con null en mapeos parciales
                .ForAllMembers(opt => opt.Condition((_, __, srcMember) => srcMember != null));

            // === Detalle ===
            CreateMap<ReciboEntity, ReciboDetailDto>()
                .ForMember(d => d.Placa, m => m.MapFrom(s => s.PlacaSnapshot))
                .ForMember(d => d.ConductorNombre, m => m.MapFrom(s => s.ConductorNombreSnapshot))
                .ForMember(d => d.Estado, m => m.MapFrom(s => (ReciboEstado)s.Estado))
                .ForAllMembers(opt => opt.Condition((_, __, srcMember) => srcMember != null));
        }
    }
}
