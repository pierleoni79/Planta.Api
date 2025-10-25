// Ruta: /Planta.Data/Configurations/op.ReciboConfiguration.cs | V1.1
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Data.Entities;

namespace Planta.Data.Configurations;

internal sealed class ReciboConfiguration : IEntityTypeConfiguration<Recibo>
{
    public void Configure(EntityTypeBuilder<Recibo> b)
    {
        b.ToTable("Recibo", "op");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("newsequentialid()");

        b.Property(x => x.Consecutivo)
            .HasColumnName("Consecutivo")
            .HasDefaultValueSql("NEXT VALUE FOR [op].[Seq_Recibo]");

        b.Property(x => x.FechaCreacion).HasColumnName("FechaCreacion");
        b.Property(x => x.VehiculoId).HasColumnName("VehiculoId");
        b.Property(x => x.PlacaSnapshot).HasMaxLength(10);
        b.Property(x => x.ConductorId);
        b.Property(x => x.ConductorNombreSnapshot).HasMaxLength(120);
        b.Property(x => x.DestinoTipo).HasColumnName("DestinoTipo");
        b.Property(x => x.EmpresaId);
        b.Property(x => x.ClienteId);
        b.Property(x => x.MaterialId);
        b.Property(x => x.Estado);
        b.Property(x => x.UsuarioCreador).HasMaxLength(64).IsRequired();
        b.Property(x => x.Observaciones).HasMaxLength(512);
        b.Property(x => x.UltimaActualizacion);
        b.Property(x => x.ReciboFisicoNumero).HasMaxLength(50);
        b.Property(x => x.NumeroGenerado).HasMaxLength(50);
        b.Property(x => x.AutoGenerado);
        b.Property(x => x.AutoGeneradoEn);
        b.Property(x => x.Activo);
        b.Property(x => x.Cantidad).HasColumnType("decimal(18,3)");
        b.Property(x => x.AlmacenOrigenId);
        b.Property(x => x.ReciboFisicoNumeroNorm).HasMaxLength(50);

        // ➕ Nuevos mapeos
        b.Property(x => x.Unidad).HasMaxLength(10);
        b.Property(x => x.IdempotencyKey).HasMaxLength(64);

        // Índices sugeridos (empresa única) — solo si usas migraciones
        // b.HasIndex(x => new { x.Estado, x.FechaCreacion }).HasDatabaseName("IX_Recibo_Estado_Fecha");
        // b.HasIndex(x => x.Consecutivo).IsUnique().HasDatabaseName("UQ_Recibo_Consecutivo");
        // b.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL").HasDatabaseName("UQ_Recibo_IdempotencyKey");
        // b.HasIndex(x => x.NumeroGenerado).IsUnique().HasFilter("[NumeroGenerado] IS NOT NULL").HasDatabaseName("UQ_Recibo_NumeroGenerado");
        // b.HasIndex(x => x.ReciboFisicoNumero).IsUnique().HasFilter("[ReciboFisicoNumero] IS NOT NULL").HasDatabaseName("UQ_Recibo_ReciboFisicoNumero");
    }
}
