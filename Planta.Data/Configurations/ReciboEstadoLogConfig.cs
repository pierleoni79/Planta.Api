using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Data.Entities;   // ✅ usar la entidad real

namespace Planta.Data.Configurations;

public sealed class ReciboEstadoLogConfig : IEntityTypeConfiguration<ReciboEstadoLog>
{
    public void Configure(EntityTypeBuilder<ReciboEstadoLog> b)
    {
        // Tabla real: op.ReciboEstadoLog
        b.ToTable("ReciboEstadoLog", "op");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();          // bigint IDENTITY

        b.Property(x => x.ReciboId).IsRequired();             // uniqueidentifier
        b.Property(x => x.Estado).IsRequired();               // byte -> tinyint
        b.Property(x => x.Cuando)
            .HasDefaultValueSql("sysdatetimeoffset()");       // datetimeoffset

        b.Property(x => x.Nota).HasMaxLength(512);
        b.Property(x => x.GPS).HasMaxLength(128);

        // Índice útil para consultas por recibo y orden por fecha
        b.HasIndex(x => new { x.ReciboId, x.Cuando })
         .HasDatabaseName("IX_ReciboEstadoLog_Recibo_Cuando");
    }
}
