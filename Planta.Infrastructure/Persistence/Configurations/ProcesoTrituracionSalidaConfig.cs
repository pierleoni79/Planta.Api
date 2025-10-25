// Ruta: /Planta.Infrastructure/Persistence/Configurations/ProcesoTrituracionSalidaConfig.cs | V1.0
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Domain.Produccion;

namespace Planta.Infrastructure.Persistence.Configurations;

public sealed class ProcesoTrituracionSalidaConfig : IEntityTypeConfiguration<ProcesoTrituracionSalida>
{
    public void Configure(EntityTypeBuilder<ProcesoTrituracionSalida> b)
    {
        b.ToTable("ProcesoTrituracionSalida");
        b.HasKey(x => x.Id);
        b.Property(x => x.ProductoId).IsRequired();
        b.Property(x => x.Cantidad).IsRequired();
        b.Property(x => x.EsMerma).IsRequired();
        b.Property(x => x.Vendible).IsRequired();
        b.HasIndex(x => new { x.ProcesoId, x.ProductoId }).IsUnique();
    }
}
