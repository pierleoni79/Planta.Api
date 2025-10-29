// Ruta: /Planta.Data/Entities/Recibo.cs | V1.5
#nullable enable
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Planta.Data.Entities;

/// <summary>
/// Entidad que mapea 1:1 la tabla op.Recibo.
/// - Los defaults/precision/índices se configuran en Configurations/ReciboConfig.
/// - ETag se calcula a partir de campos (Consecutivo, UltimaActualizacion, FechaCreacion, Estado) en la capa App/API.
/// </summary>
public sealed class Recibo
{
    // Identidad y tiempos
    public Guid Id { get; set; }                          // DEFAULT newsequentialid()
    public int Consecutivo { get; set; }                  // DEFAULT NEXT VALUE FOR [op].[Seq_Recibo]
    public DateTimeOffset FechaCreacion { get; set; }     // DEFAULT sysdatetimeoffset()
    public DateTimeOffset? UltimaActualizacion { get; set; }

    // Claves / relaciones
    public int EmpresaId { get; set; }
    public int VehiculoId { get; set; }
    public int MaterialId { get; set; }
    public int AlmacenOrigenId { get; set; }
    public int? ClienteId { get; set; }
    public int? ConductorId { get; set; }

    // Snapshots (texto)
    public string? PlacaSnapshot { get; set; }            // nvarchar(20)
    public string? ConductorNombreSnapshot { get; set; }  // nvarchar(240)

    // Estado y destino
    public byte Estado { get; set; }                      // tinyint → 10/12/20/30/40/90/99
    public byte DestinoTipo { get; set; }                 // tinyint → 1=Planta, 2=ClienteDirecto

    // Auditoría / varios
    public string UsuarioCreador { get; set; } = "api";   // nvarchar(128) NOT NULL
    public string? Observaciones { get; set; }            // nvarchar(1024)
    public bool Activo { get; set; } = true;              // DEFAULT (1)

    // Cantidad
    public decimal Cantidad { get; set; }                 // decimal(18,3)

    // Idempotencia y numeraciones
    public string? IdempotencyKey { get; set; }           // nvarchar(128)
    public string? ReciboFisicoNumero { get; set; }       // nvarchar(100)
    public string? NumeroGenerado { get; set; }           // nvarchar(100)
    public bool AutoGenerado { get; set; }                // DEFAULT (0)
    public DateTimeOffset? AutoGeneradoEn { get; set; }
    public string? ReciboFisicoNumeroNorm { get; set; }   // nvarchar(100)

    // Proyección no persistida
    [NotMapped] public string? Unidad { get; set; }

    // Nota: SIN navegaciones a tipos legacy (Proceso/ProcesoDet) para evitar doble mapeo.
}
