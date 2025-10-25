// Ruta: /Planta.Data/Configurations/ReciboEstadoLogConfig.cs | V2.2
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// ✅ Usa la entidad REAL de datos.
// Si tu clase está en otro namespace, deja AMBOS usings y el compilador tomará el correcto.
using Planta.Data.Entities;
using Planta.Domain.Operaciones;

namespace Planta.Data.Configurations;

public sealed class ReciboEstadoLogConfig : IEntityTypeConfiguration<ReciboEstadoLog>
{
    public void Configure(EntityTypeBuilder<ReciboEstadoLog> b)
    {
        // Tabla real: op.ReciboEstadoLog
        b.ToTable("ReciboEstadoLog", "op");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();                 // bigint IDENTITY

        b.Property(x => x.ReciboId).IsRequired();                    // uniqueidentifier
        b.Property(x => x.Estado).HasColumnType("tinyint").IsRequired();

        b.Property(x => x.Cuando)
            .HasColumnType("datetimeoffset")
            .HasDefaultValueSql("sysdatetimeoffset()");

        b.Property(x => x.Nota).HasMaxLength(512);
        b.Property(x => x.GPS).HasMaxLength(128);

        // Índices
        b.HasIndex(x => x.ReciboId);
        b.HasIndex(x => new { x.ReciboId, x.Cuando });
    }
}
