// Ruta: /Planta.Data/Entities/Recibo.cs | V1.2
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Planta.Data.Entities;

public class Recibo
{
    public Guid Id { get; set; }                    // PK (newsequentialid())
    public int Consecutivo { get; set; }            // DEFAULT NEXT VALUE FOR op.Seq_Recibo
    public DateTimeOffset FechaCreacion { get; set; }

    public int VehiculoId { get; set; }
    public string? PlacaSnapshot { get; set; }      // nvarchar(20)

    public int? ConductorId { get; set; }
    public string? ConductorNombreSnapshot { get; set; } // nvarchar(240)

    public byte DestinoTipo { get; set; }           // 1=Planta, 2=ClienteDirecto
    public int EmpresaId { get; set; }
    public int? ClienteId { get; set; }
    public int MaterialId { get; set; }

    public byte Estado { get; set; }                // 0,10,12,20,30,40,90,99
    public string UsuarioCreador { get; set; } = "api"; // nvarchar(128)
    public string? Observaciones { get; set; }      // nvarchar(1024)
    public DateTimeOffset? UltimaActualizacion { get; set; }

    public bool Activo { get; set; }
    public decimal Cantidad { get; set; }           // decimal(18,3)
    public int AlmacenOrigenId { get; set; }        // NOT NULL en BD

    [NotMapped]
    public string? Unidad { get; set; }             // Solo proyección (no existe en op.Recibo)

    public string? IdempotencyKey { get; set; }     // nvarchar(128)

    // Campos mapeados en config:
    public string? ReciboFisicoNumero { get; set; }     // nvarchar(100)
    public string? NumeroGenerado { get; set; }         // nvarchar(100)
    public bool AutoGenerado { get; set; }
    public DateTimeOffset? AutoGeneradoEn { get; set; }
    public string? ReciboFisicoNumeroNorm { get; set; } // nvarchar(100)
}
