// Ruta: /Planta.Data/Configurations/ProcesoTrituracionSalidaConfig.cs | V2.6
#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Domain.Produccion;

namespace Planta.Data.Configurations;

public sealed class ProcesoTrituracionSalidaConfig : IEntityTypeConfiguration<ProcesoTrituracionSalida>
{
    public void Configure(EntityTypeBuilder<ProcesoTrituracionSalida> b)
    {
        // ===== Tabla + CHECK =====
        b.ToTable("ProcesoDet", "prd", tb =>
        {
            // Valida datos: no se permiten cantidades <= 0
            tb.HasCheckConstraint("CK_ProcesoDet_CantidadM3_Pos", "[CantidadM3] > 0");
        });

        // ===== PK =====
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd(); // IDENTITY

        // ===== Claves =====
        b.Property(x => x.ProcesoId).IsRequired();
        b.Property(x => x.ProductoId).IsRequired();

        // ===== Cantidad (propiedad sombra mapeada a la columna real) =====
        b.Property<decimal>("CantidadM3")
         .HasColumnName("CantidadM3")
         .HasPrecision(18, 3)
         .IsRequired();

        // ===== Índice único (coincide con tu esquema) =====
        b.HasIndex(x => new { x.ProcesoId, x.ProductoId })
         .IsUnique()
         .HasDatabaseName("IX_prd_ProcesoDet_Proceso_Producto");

        // ===== Relación con Proceso (consistente con ProcesoConfig) =====
        b.HasOne(s => s.Proceso!)
         .WithMany(p => p.Salidas)
         .HasForeignKey(s => s.ProcesoId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
