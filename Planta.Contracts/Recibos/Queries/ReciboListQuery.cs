// Ruta: /Planta.Contracts/Recibos/Queries/ReciboListQuery.cs | V1.1
#nullable enable
using Planta.Contracts.Common;
using Planta.Contracts.Enums;

namespace Planta.Contracts.Recibos.Queries;

public sealed record ReciboListFilter(
    int? EmpresaId = null,
    ReciboEstado? Estado = null,
    DestinoTipo? DestinoTipo = null,
    DateTime? FechaDesdeUtc = null,
    DateTime? FechaHastaUtc = null,
    int? VehiculoId = null,
    int? ConductorId = null,
    int? ClienteId = null,
    int? MaterialId = null,
    string? Search = null
);

public sealed record ReciboListQuery(
    PagedRequest Paging,
    ReciboListFilter Filter
);
