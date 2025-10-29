// Ruta: /Planta.Data/Configurations/ProcesoTrituracionConfig.cs | V2.2
#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Domain.Produccion;

namespace Planta.Data.Configurations;

public sealed class ProcesoTrituracionConfig : IEntityTypeConfiguration<ProcesoTrituracion>
{
    public void Configure(EntityTypeBuilder<ProcesoTrituracion> b)
    {
        // ===== Tabla y PK =====
        b.ToTable("Proceso", "prd");
        b.HasKey(x => x.Id);

        // ===== Claves / columnas básicas =====
        b.Property(x => x.ReciboId).IsRequired();
        b.HasIndex(x => x.ReciboId);

        // EntradaM3 (DB) ← PesoEntrada (Dominio)
        b.Property(x => x.PesoEntrada)
            .HasColumnName("EntradaM3")
            .HasPrecision(18, 3)
            .IsRequired();

        // Sombras para columnas presentes en BD pero no en Dominio
        b.Property<int?>("RecetaId")
            .HasColumnName("RecetaId")
            .IsRequired(false);

        b.Property<byte>("Estado")
            .HasColumnName("Estado")
            .HasColumnType("tinyint")
            .HasDefaultValue((byte)0);

        b.Property<string?>("Observacion")
            .HasColumnName("Observacion")
            .HasMaxLength(1024);

        // CreadoEn (DB) ← CreatedAtUtc (Dominio)
        b.Property(x => x.CreatedAtUtc)
            .HasColumnName("CreadoEn")
            .HasDefaultValueSql("sysdatetimeoffset()");

        // Índice útil para consultas por recibo y fecha
        b.HasIndex(x => new { x.ReciboId, x.CreatedAtUtc });

        // Relación con detalles (prd.ProcesoDet)
        b.HasMany(x => x.Salidas)
            .WithOne(s => s.Proceso!)
            .HasForeignKey(s => s.ProcesoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignorar propiedades de dominio que no persisten
        b.Ignore(x => x.PesoSalida);
        b.Ignore(x => x.Residuos);
        b.Ignore(x => x.BalancePorc);
        b.Ignore(x => x.Epsilon);
        b.Ignore(x => x.DeltaMin);
    }
}
