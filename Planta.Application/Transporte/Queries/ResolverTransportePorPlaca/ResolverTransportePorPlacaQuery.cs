// Ruta: /Planta.Application/Transporte/Queries/ResolverTransportePorPlaca/ResolverTransportePorPlacaQuery.cs | V1.0
#nullable enable
using MediatR;
using Planta.Contracts.Transporte;

namespace Planta.Application.Transporte.Queries.ResolverTransportePorPlaca;

public sealed record ResolverTransportePorPlacaQuery(string Placa) : IRequest<TransporteResolucionDto?>;
