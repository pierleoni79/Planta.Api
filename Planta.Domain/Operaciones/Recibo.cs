// Ruta: /Planta.Domain/Operaciones/Recibo.cs | V1.0
using System;

namespace Planta.Domain.Operaciones
{
    // Mapea a op.Recibo (solo campos requeridos aquí)
    public sealed class Recibo
    {
        public Guid Id { get; set; }                             // uniqueidentifier
        public byte Estado { get; set; }                         // tinyint
        public DateTimeOffset UltimaActualizacion { get; set; }  // datetimeoffset
    }
}
