// Ruta: /Planta.Contracts/Recibos/Requests/ReciboCreateRequest.cs | V1.4
#nullable enable
using System;
using Planta.Contracts.Enums;

namespace Planta.Contracts.Recibos.Requests;

/// <summary>
/// Solicitud para crear un Recibo. Soporta el flujo mínimo (crear)
/// y deja campos opcionales para escenarios extendidos.
/// </summary>
public sealed class ReciboCreateRequest
{
    // ---------------- Identificación / idempotencia ----------------
    public Guid Id { get; init; }           // usado por CreateNew
    public string IdempotencyKey { get; init; } = default!;

    // ---------------- Contexto ----------------
    public int EmpresaId { get; init; }
    public DestinoTipo DestinoTipo { get; init; }

    // ---------------- Maestro/destino (opcionales si se vinculan luego) ----------------
    public int? ClienteId { get; init; }
    public int? PlantaId { get; init; }

    // Material / unidad (opcionales si se registran luego)
    public int? MaterialId { get; init; }    // ← opcional para no romper el flujo por pasos
    public string? Unidad { get; init; }    // p.ej. "m3" | "viaje"

    // ---------------- Cantidades ----------------
    public decimal Cantidad { get; init; }

    // ---------------- Snapshots opcionales ----------------
    public string? PlacaSnapshot { get; init; }
    public string? ConductorNombreSnapshot { get; init; }

    // ---------------- Transporte opcional (para asegurar historial si se envían) ----------------
    public int? VehiculoId { get; init; }
    public int? ConductorId { get; init; }

    // ---------------- Documentos opcionales ----------------
    public string? ReciboFisicoNumero { get; init; }

    /// <summary>
    /// Ctor vacío para serialización/model binding.
    /// </summary>
    public ReciboCreateRequest() { }

    /// <summary>
    /// Ctor de conveniencia que coincide con las llamadas actuales desde Mobile/API.
    /// </summary>
    public ReciboCreateRequest(
        Guid id,
        int empresaId,
        DestinoTipo destinoTipo,
        decimal cantidad,
        string idempotencyKey,
        string? placaSnapshot = null,
        string? conductorNombreSnapshot = null,
        string? reciboFisicoNumero = null)
    {
        Id = id;
        EmpresaId = empresaId;
        DestinoTipo = destinoTipo;
        Cantidad = cantidad;
        IdempotencyKey = idempotencyKey ?? throw new ArgumentNullException(nameof(idempotencyKey));
        PlacaSnapshot = placaSnapshot;
        ConductorNombreSnapshot = conductorNombreSnapshot;
        ReciboFisicoNumero = reciboFisicoNumero;
    }
}
