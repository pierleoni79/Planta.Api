// Ruta: /Planta.Application/Recibos/Mapping/ReciboProfile.cs | V1.1
#nullable enable
using Planta.Contracts.Recibos;
using System.Diagnostics.Contracts;

namespace Planta.Application.Recibos.Mapping;

public sealed class ReciboProfile : Profile
{
    public ReciboProfile()
    {
        // Domain -> Contracts
        CreateMap<Planta.Domain.Recibos.Recibo, ReciboDto>()
            .ForCtorParam("Estado", opt => opt.MapFrom(s => (Contracts.Enums.ReciboEstado)s.Estado))
            .ForCtorParam("DestinoTipo", opt => opt.MapFrom(s => (Contracts.Enums.DestinoTipo)s.DestinoTipo))
            .ForCtorParam("ETag", opt => opt.MapFrom(s => s.ComputeWeakETag()));

        CreateMap<Planta.Domain.Recibos.Recibo, ReciboListItemDto>()
            .ForCtorParam("Estado", opt => opt.MapFrom(s => (Contracts.Enums.ReciboEstado)s.Estado))
            .ForCtorParam("DestinoTipo", opt => opt.MapFrom(s => (Contracts.Enums.DestinoTipo)s.DestinoTipo))
            .ForCtorParam("ETag", opt => opt.MapFrom(s => s.ComputeWeakETag()));

        CreateMap<Planta.Domain.Recibos.ReciboEstadoLog, ReciboEstadoLogDto>()
            .ForCtorParam("Estado", opt => opt.MapFrom(s => (Contracts.Enums.ReciboEstado)s.Estado));
    }
}
