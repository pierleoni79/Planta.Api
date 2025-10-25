// Ruta: /Planta.Infrastructure/Persistence/Configurations/ProcesoTrituracionConfig.cs | V1.0
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Domain.Produccion;

namespace Planta.Infrastructure.Persistence.Configurations;

public sealed class ProcesoTrituracionConfig : IEntityTypeConfiguration<ProcesoTrituracion>
{
    public void Configure(EntityTypeBuilder<ProcesoTrituracion> b)
    {
        b.ToTable("ProcesoTrituracion");
        b.HasKey(x => x.Id);
        b.Property(x => x.ReciboId).IsRequired();
        b.Property(x => x.PesoEntrada).IsRequired();
        b.Property(x => x.PesoSalida).IsRequired();
        b.Property(x => x.Residuos).IsRequired();
        b.Property(x => x.BalancePorc).HasPrecision(9, 3);
        b.Property(x => x.Epsilon).HasPrecision(9, 4).HasDefaultValue(0.01);
        b.Property(x => x.DeltaMin).HasPrecision(9, 3).HasDefaultValue(0.1);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
        b.HasMany(x => x.Salidas).WithOne(s => s.Proceso).HasForeignKey(s => s.ProcesoId);
        b.HasIndex(x => x.ReciboId);
    }
}
