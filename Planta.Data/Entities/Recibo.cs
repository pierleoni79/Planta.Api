// Ruta: /Planta.Data/Entities/Recibo.cs | V1.3
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Planta.Data.Entities;

/// <summary>
/// Entidad op.Recibo (DB). Mapea columnas reales y mantiene snapshots para placa/conductor.
/// Nota: precision/longitudes y defaults se definen en Configurations/*.cs
/// </summary>
public sealed class Recibo
{
    // --- Identidad & tiempos ---
    public Guid Id { get; set; }                        // PK (DEFAULT newsequentialid())
    public int Consecutivo { get; set; }                // DEFAULT NEXT VALUE FOR [op].[Seq_Recibo]
    public DateTimeOffset FechaCreacion { get; set; }   // set en SaveChanges (Added)
    public DateTimeOffset? UltimaActualizacion { get; set; } // set en SaveChanges (Modified)

    // --- Claves de negocio / relación ---
    public int EmpresaId { get; set; }
    public int VehiculoId { get; set; }                 // NOT NULL
    public int MaterialId { get; set; }                 // NOT NULL
    public int AlmacenOrigenId { get; set; }            // NOT NULL
    public int? ClienteId { get; set; }                 // NULL
    public int? ConductorId { get; set; }               // NULL

    // --- Snapshots (texto) ---
    public string? PlacaSnapshot { get; set; }          // nvarchar(20)
    public string? ConductorNombreSnapshot { get; set; }// nvarchar(240)

    // --- Estado & destino ---
    public byte Estado { get; set; }                    // tinyint -> 10/12/20/30/40/90/99
    public byte DestinoTipo { get; set; }               // tinyint -> 0=Planta, 1=ClienteDirecto

    // --- Auditoría & observaciones ---
    public string UsuarioCreador { get; set; } = "api"; // nvarchar(128) NOT NULL
    public string? Observaciones { get; set; }          // nvarchar(1024)
    public bool Activo { get; set; } = true;            // DEFAULT (1) en BD

    // --- Cantidades ---
    public decimal Cantidad { get; set; }               // decimal(18,3)

    // --- Idempotencia & numeraciones ---
    public string? IdempotencyKey { get; set; }         // nvarchar(128)  (formato "scope:key")
    public string? ReciboFisicoNumero { get; set; }     // nvarchar(100)
    public string? NumeroGenerado { get; set; }         // nvarchar(100)
    public bool AutoGenerado { get; set; }              // DEFAULT (0)
    public DateTimeOffset? AutoGeneradoEn { get; set; } // NULL
    public string? ReciboFisicoNumeroNorm { get; set; } // nvarchar(100)

    // --- Proyección no persistida (si la usas en queries de UI) ---
    [NotMapped] public string? Unidad { get; set; }

    // --- Navegación ---
    public ICollection<Proceso> Procesos { get; set; } = new List<Proceso>();
}
