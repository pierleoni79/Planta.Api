// Ruta: /Planta.Mobile/Models/Recibos/NuevoReciboForm.cs | V1.2
#nullable enable
using Planta.Contracts.Enums;

namespace Planta.Mobile.Models.Recibos;

public sealed class NuevoReciboForm
{
    // Identidad/empresa
    public Guid Id { get; set; }
    public int EmpresaId { get; set; }

    // Transporte
    public int? VehiculoId { get; set; }
    public string? Placa { get; set; }
    public int? ConductorId { get; set; }
    public string? ConductorNombreSnapshot { get; set; }

    // Negocio
    public DestinoTipo Destino { get; set; }              // ClienteDirecto o Planta
    public int MaterialId { get; set; }
    public decimal Cantidad { get; set; }
    public string? Unidad { get; set; }                   // m³, etc.

    // Origen/Destino complementos
    public int? ClienteId { get; set; }
    public int? PlantaId { get; set; }
    public int? AlmacenOrigenId { get; set; }

    // Metadatos
    public string? ReciboFisicoNumero { get; set; }
    public string? IdempotencyKey { get; set; }
}
