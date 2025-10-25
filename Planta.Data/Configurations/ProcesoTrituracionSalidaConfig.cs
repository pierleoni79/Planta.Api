// Ruta: /Planta.Data/Configurations/ProcesoTrituracionSalidaConfig.cs | V2.1
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Domain.Produccion;

namespace Planta.Data.Configurations;

public sealed class ProcesoTrituracionSalidaConfig : IEntityTypeConfiguration<ProcesoTrituracionSalida>
{
    public void Configure(EntityTypeBuilder<ProcesoTrituracionSalida> b)
    {
        // Tabla real: prd.ProcesoDet
        b.ToTable("ProcesoDet", "prd");

        b.HasKey(x => x.Id);

        b.Property(x => x.ProcesoId).IsRequired();
        b.Property(x => x.ProductoId).IsRequired();

        // La entidad de dominio NO tiene 'CantidadM3', así que lo mapeamos como "shadow property".
        // (Si luego agregas una propiedad CLR, cambia esto por b.Property(x => x.<tuPropiedad>).HasColumnName("CantidadM3")...)
        b.Property<decimal>("CantidadM3")
            .HasPrecision(18, 3)
            .IsRequired();

        b.HasIndex(x => new { x.ProcesoId, x.ProductoId });
    }
}
