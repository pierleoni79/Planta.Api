// Ruta: /Planta.Data/Configurations/ProcesoTrituracionConfig.cs | V2.1
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Domain.Produccion;

namespace Planta.Data.Configurations;

public sealed class ProcesoTrituracionConfig : IEntityTypeConfiguration<ProcesoTrituracion>
{
    public void Configure(EntityTypeBuilder<ProcesoTrituracion> b)
    {
        // Tabla real
        b.ToTable("Proceso", "prd");

        b.HasKey(x => x.Id);

        b.Property(x => x.ReciboId).IsRequired();
        b.HasIndex(x => x.ReciboId);

        // Tu entidad NO tiene RecetaId/Estado/EntradaM3/Observacion.
        // mapeamos así:
        // - EntradaM3 (DB)  ← PesoEntrada (CLR)
        // - RecetaId, Estado y Observacion como "shadow properties" (existen en DB, no en la clase)
        b.Property(x => x.PesoEntrada)
            .HasColumnName("EntradaM3")
            .HasPrecision(18, 3)
            .IsRequired();

        b.Property<int?>("RecetaId");                       // nullable en BD
        b.Property<byte>("Estado").HasDefaultValue((byte)0);
        b.Property<string?>("Observacion").HasMaxLength(1024);

        // Otras columnas calculadas/temporales que tienes en el dominio pero NO en BD
        b.Ignore(x => x.PesoSalida);
        b.Ignore(x => x.Residuos);
        b.Ignore(x => x.BalancePorc);
        b.Ignore(x => x.Epsilon);
        b.Ignore(x => x.DeltaMin);

        // CreadoEn (DB) ← CreatedAtUtc (CLR)
        b.Property(x => x.CreatedAtUtc)
            .HasColumnName("CreadoEn")
            .HasDefaultValueSql("sysdatetimeoffset()");

        // Relación con detalles (prd.ProcesoDet)
        b.HasMany(x => x.Salidas)
            .WithOne(s => s.Proceso!)
            .HasForeignKey(s => s.ProcesoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
