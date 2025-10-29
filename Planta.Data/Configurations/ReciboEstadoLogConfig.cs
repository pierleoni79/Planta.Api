// Ruta: /Planta.Data/Configurations/ReciboEstadoLogConfig.cs | V1.1
#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Data.Entities;   // ReciboEstadoLog, Recibo

namespace Planta.Data.Configurations;

public sealed class ReciboEstadoLogConfig : IEntityTypeConfiguration<ReciboEstadoLog>
{
    public void Configure(EntityTypeBuilder<ReciboEstadoLog> b)
    {
        // Tabla real
        b.ToTable("ReciboEstadoLog", "op");

        // PK (bigint IDENTITY)
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // Campos
        b.Property(x => x.ReciboId)
            .IsRequired();

        // tinyint en BD
        b.Property(x => x.Estado)
            .HasColumnType("tinyint")
            .IsRequired();

        // datetimeoffset con default
        b.Property(x => x.Cuando)
            .HasDefaultValueSql("sysdatetimeoffset()")
            .IsRequired();

        b.Property(x => x.Nota)
            .HasMaxLength(512);

        b.Property(x => x.GPS)
            .HasMaxLength(128);

        // FK explícita a op.Recibo SIN cascada (preserva el historial si borran Recibo)
        b.HasOne<Recibo>()
            .WithMany() // no tienes navegación en Recibo; si la agregas, cámbiala aquí
            .HasForeignKey(x => x.ReciboId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice útil para lecturas: por Recibo y orden temporal
        b.HasIndex(x => new { x.ReciboId, x.Cuando })
         .HasDatabaseName("IX_ReciboEstadoLog_Recibo_Cuando");
    }
}
