// Ruta: /Planta.Data/Entities/ProcesoDet.cs | V1.1
#nullable enable
using System.ComponentModel.DataAnnotations.Schema;

namespace Planta.Data.Entities
{
    /// <summary>
    /// Detalle de un proceso de trituración (prd.ProcesoDet).
    /// </summary>
    [Table("ProcesoDet", Schema = "prd")]
    public sealed class ProcesoDet
    {
        public int Id { get; set; }                 // PK (IDENTITY)
        public int ProcesoId { get; set; }          // FK → prd.Proceso.Id
        public int ProductoId { get; set; }         // FK → cat.Producto.Id (en tu esquema)

        [Column(TypeName = "decimal(18,3)")]
        public decimal CantidadM3 { get; set; }     // cantidad generada en m3

        // Navegación opcional (útil para Include/relaciones por convención)
        public Proceso? Proceso { get; set; }
    }
}
