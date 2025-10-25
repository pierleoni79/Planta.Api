// Ruta: /Planta.Data/Entities/Recibo.cs | V1.1
using System;

namespace Planta.Data.Entities;

public class Recibo
{
    public Guid Id { get; set; }                    // PK (newsequentialid())
    public int Consecutivo { get; set; }            // DEFAULT NEXT VALUE FOR op.Seq_Recibo
    public DateTimeOffset FechaCreacion { get; set; }

    public int VehiculoId { get; set; }
    public string? PlacaSnapshot { get; set; }

    public int? ConductorId { get; set; }
    public string? ConductorNombreSnapshot { get; set; } // <— agregado

    public byte DestinoTipo { get; set; }           // 1=Planta, 2=ClienteDirecto
    public int EmpresaId { get; set; }
    public int? ClienteId { get; set; }
    public int MaterialId { get; set; }

    public byte Estado { get; set; }                // 0,10,12,20,30,40,90,99
    public string UsuarioCreador { get; set; } = "api";
    public string? Observaciones { get; set; }
    public DateTimeOffset? UltimaActualizacion { get; set; }

    public bool Activo { get; set; }
    public decimal Cantidad { get; set; }           // m3
    public int AlmacenOrigenId { get; set; }

    public string? Unidad { get; set; }             // <— usado por el servicio
    public string? IdempotencyKey { get; set; }     // <— usado por el servicio/parche SQL

    // Campos que tu config mapea explícitamente:
    public string? ReciboFisicoNumero { get; set; }     // len 50
    public string? NumeroGenerado { get; set; }         // len 50
    public bool AutoGenerado { get; set; }
    public DateTimeOffset? AutoGeneradoEn { get; set; }
    public string? ReciboFisicoNumeroNorm { get; set; } // len 50
}
