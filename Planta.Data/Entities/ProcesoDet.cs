// Ruta: /Planta.Data/Entities/ProcesoDet.cs | V1.2 (LEGACY — NO EF)
#nullable enable
using System;

namespace Planta.Data.Entities;

/// <summary>
/// LEGACY: snapshot del esquema prd.ProcesoDet.
/// NO se mapea con EF. El mapeo real lo hace
/// Planta.Domain.Produccion.ProcesoTrituracionSalida
/// en ProcesoTrituracionSalidaConfig (CantidadM3 = shadow property).
/// </summary>
[Obsolete("Legacy. Usa Planta.Domain.Produccion.ProcesoTrituracionSalida*", error: false)]
public sealed class ProcesoDet
{
    public int Id { get; set; }          // int IDENTITY en BD
    public int ProcesoId { get; set; }   // FK → prd.Proceso.Id
    public int ProductoId { get; set; }  // FK → cat.Producto.Id
    public decimal CantidadM3 { get; set; } // (18,3) — sólo informativo aquí
}
