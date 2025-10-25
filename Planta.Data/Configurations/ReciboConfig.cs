// Ruta: /Planta.Data/Configurations/ReciboConfig.cs | V2.1
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Data.Entities;          // ✅ entidad completa usada por Planta.Data.Context

namespace Planta.Data.Configurations;

public sealed class ReciboConfig : IEntityTypeConfiguration<Recibo>
{
    public void Configure(EntityTypeBuilder<Recibo> b)
    {
        // Tabla real: op.Recibo
        b.ToTable("Recibo", "op");

        b.HasKey(x => x.Id);

        // Claves de negocio / restricciones
        b.Property(x => x.EmpresaId).IsRequired();
        b.Property(x => x.Consecutivo).IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Consecutivo }).IsUnique();

        // Campos principales
        b.Property(x => x.FechaCreacion).IsRequired();
        b.Property(x => x.Estado).IsRequired();          // tinyint
        b.Property(x => x.DestinoTipo).IsRequired();     // tinyint

        b.Property(x => x.VehiculoId).IsRequired();
        b.Property(x => x.ConductorId).IsRequired(false);

        b.Property(x => x.PlacaSnapshot).HasMaxLength(20);
        b.Property(x => x.ConductorNombreSnapshot).HasMaxLength(240);

        b.Property(x => x.ClienteId).IsRequired(false);
        b.Property(x => x.MaterialId).IsRequired();

        b.Property(x => x.Observaciones).HasMaxLength(1024);
        b.Property(x => x.UltimaActualizacion).IsRequired(false);

        b.Property(x => x.ReciboFisicoNumero).HasMaxLength(100);
        b.Property(x => x.ReciboFisicoNumeroNorm).HasMaxLength(100);
        b.Property(x => x.NumeroGenerado).HasMaxLength(100);
        b.Property(x => x.IdempotencyKey).HasMaxLength(128);

        b.Property(x => x.AutoGenerado).IsRequired();
        b.Property(x => x.AutoGeneradoEn).IsRequired(false);
        b.Property(x => x.Activo).IsRequired();

        b.Property(x => x.Cantidad)
            .HasPrecision(18, 3)
            .IsRequired();

        // En tu diccionario, AlmacenOrigenId se usa siempre → Required
        b.Property(x => x.AlmacenOrigenId).IsRequired();

        // Índices de consulta comunes
        b.HasIndex(x => x.FechaCreacion);
        b.HasIndex(x => x.ClienteId);
        b.HasIndex(x => x.Estado);
    }
}
