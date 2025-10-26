// Ruta: /Planta.Data/Entities/Proceso.cs | V1.0
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Planta.Data.Entities
{
    [Table("Proceso", Schema = "prd")]
    public sealed class Proceso
    {
        public int Id { get; set; }
        public Guid ReciboId { get; set; }
        public int RecetaId { get; set; }
        public byte Estado { get; set; }  // prd.Proceso.Estado (tinyint). Úsalo si luego manejas estados del proceso.
        [Column(TypeName = "decimal(18,3)")]
        public decimal EntradaM3 { get; set; }
        public string? Observacion { get; set; }
        public DateTimeOffset CreadoEn { get; set; }
    }
}
