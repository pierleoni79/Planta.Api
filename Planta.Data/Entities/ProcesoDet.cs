// Ruta: /Planta.Data/Entities/ProcesoDet.cs | V1.0
using System.ComponentModel.DataAnnotations.Schema;

namespace Planta.Data.Entities
{
    [Table("ProcesoDet", Schema = "prd")]
    public sealed class ProcesoDet
    {
        public int Id { get; set; }
        public int ProcesoId { get; set; }
        public int ProductoId { get; set; }
        [Column(TypeName = "decimal(18,3)")]
        public decimal CantidadM3 { get; set; }
    }
}
