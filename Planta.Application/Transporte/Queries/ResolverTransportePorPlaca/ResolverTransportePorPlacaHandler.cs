// Ruta: /Planta.Application/Transporte/Queries/ResolverTransportePorPlaca/ResolverTransportePorPlacaHandler.cs | V1.1 (fix: usar Abstractions)
#nullable enable
using MediatR;
using Planta.Contracts.Transporte;
using Planta.Application.Transporte.Abstractions; // ← cambio clave

namespace Planta.Application.Transporte.Queries.ResolverTransportePorPlaca;

public sealed class ResolverTransportePorPlacaHandler(ITransporteReadStore rs)
    : IRequestHandler<ResolverTransportePorPlacaQuery, TransporteResolucionDto?>
{
    public Task<TransporteResolucionDto?> Handle(ResolverTransportePorPlacaQuery request, CancellationToken ct)
        => rs.ResolverTransportePorPlacaAsync(request.Placa, ct);
}
