// Ruta: /Planta.Data/Configurations/ReciboEstadoLogConfig.cs | V1.2
#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Data.Entities;

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
        b.Property(x => x.ReciboId).IsRequired();

        b.Property(x => x.Estado)
            .HasColumnType("tinyint")
            .IsRequired();

        b.Property(x => x.Cuando)
            .HasDefaultValueSql("sysdatetimeoffset()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        b.Property(x => x.Nota)
            .HasMaxLength(512)
            .IsRequired(false);

        b.Property(x => x.GPS)
            .HasMaxLength(128)
            .IsRequired(false);

        // FK explícita a op.Recibo SIN cascada (preserva historial)
        b.HasOne<Recibo>()
            .WithMany() // si luego agregas navegación en Recibo, cámbialo aquí
            .HasForeignKey(x => x.ReciboId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_ReciboEstadoLog_Recibo");

        // Índice por Recibo y orden temporal
        b.HasIndex(x => new { x.ReciboId, x.Cuando })
         .HasDatabaseName("IX_ReciboEstadoLog_Recibo_Cuando");
    }
}
