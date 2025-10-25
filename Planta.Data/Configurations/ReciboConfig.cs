// Ruta: /Planta.Data/Configurations/ReciboConfig.cs | V2.1
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Data.Entities;

namespace Planta.Data.Configurations;

public sealed class ReciboConfig : IEntityTypeConfiguration<Recibo>
{
    public void Configure(EntityTypeBuilder<Recibo> b)
    {
        // Tabla real
        b.ToTable("Recibo", "op");

        b.HasKey(x => x.Id);

        // Clave única (EmpresaId, Consecutivo)
        b.HasIndex(x => new { x.EmpresaId, x.Consecutivo }).IsUnique();

        // Requeridos / opcionales (idéntico al esquema)
        b.Property(x => x.EmpresaId).IsRequired();
        b.Property(x => x.Consecutivo).IsRequired();
        b.Property(x => x.FechaCreacion).IsRequired();
        b.Property(x => x.Estado).IsRequired();
        b.Property(x => x.DestinoTipo).IsRequired();

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

        // 👇 evita el warning de truncamiento
        b.Property(x => x.Cantidad).HasPrecision(18, 3).IsRequired();

        // En tu BD es NOT NULL, mantenlo requerido
        b.Property(x => x.AlmacenOrigenId).IsRequired();

        // Búsquedas
        b.HasIndex(x => x.FechaCreacion);
        b.HasIndex(x => x.ClienteId);
        b.HasIndex(x => x.Estado);
    }
}
