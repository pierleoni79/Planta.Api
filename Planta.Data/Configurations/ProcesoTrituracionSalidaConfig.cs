// Ruta: /Planta.Data/Configurations/ProcesoTrituracionSalidaConfig.cs | V2.2
#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Domain.Produccion;

namespace Planta.Data.Configurations;

public sealed class ProcesoTrituracionSalidaConfig : IEntityTypeConfiguration<ProcesoTrituracionSalida>
{
    public void Configure(EntityTypeBuilder<ProcesoTrituracionSalida> b)
    {
        b.ToTable("ProcesoDet", "prd");
        b.HasKey(x => x.Id);

        b.Property(x => x.ProcesoId).IsRequired();
        b.Property(x => x.ProductoId).IsRequired();

        // Sombras: Dominio no tiene la propiedad concreta
        b.Property<decimal>("CantidadM3")
            .HasColumnName("CantidadM3")
            .HasPrecision(18, 3)
            .IsRequired();

        b.HasIndex(x => new { x.ProcesoId, x.ProductoId });
    }
}
