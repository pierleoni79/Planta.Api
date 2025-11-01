// Ruta: /Planta.Contracts/Recibos/Requests/ReciboCreateRequest.cs | V1.3-fix
#nullable enable
using System;
using Planta.Contracts.Enums;

namespace Planta.Contracts.Recibos.Requests;

public sealed class ReciboCreateRequest
{
    // Identificación / seguridad
    public Guid Id { get; init; }                      // lo usas en CreateNew
    public string IdempotencyKey { get; init; } = default!;

    // Contexto
    public int EmpresaId { get; init; }
    public DestinoTipo DestinoTipo { get; init; }

    // Maestro/destino
    public int? ClienteId { get; init; }
    public int? PlantaId { get; init; }
    public int MaterialId { get; init; }

    // Cantidades
    public string? Unidad { get; init; }
    public decimal Cantidad { get; init; }

    // Snapshots opcionales
    public string? PlacaSnapshot { get; init; }
    public string? ConductorNombreSnapshot { get; init; }

    // ⬇️ NUEVO: opcionales para poder asegurar el historial vehículo–conductor
    public int? VehiculoId { get; init; }
    public int? ConductorId { get; init; }

    // Documentos opcionales
    public string? ReciboFisicoNumero { get; init; }
}
