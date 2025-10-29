// Ruta: /Planta.Data/Configurations/ReciboConfig.cs | V1.4
#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Data.Entities;

namespace Planta.Data.Configurations;

public sealed class ReciboConfig : IEntityTypeConfiguration<Recibo>
{
    public void Configure(EntityTypeBuilder<Recibo> b)
    {
        // ===== Tabla y PK =====
        b.ToTable("Recibo", "op");
        b.HasKey(x => x.Id);

        // Unicidad por empresa+consecutivo
        b.HasIndex(x => new { x.EmpresaId, x.Consecutivo }).IsUnique();

        // ===== Defaults y generación =====
        b.Property(x => x.Id)
            .HasDefaultValueSql("newsequentialid()")
            .ValueGeneratedOnAdd();

        b.Property(x => x.Consecutivo)
            .HasDefaultValueSql("NEXT VALUE FOR [op].[Seq_Recibo]")
            .ValueGeneratedOnAdd();

        b.Property(x => x.FechaCreacion)
            .HasDefaultValueSql("sysdatetimeoffset()")
            .IsRequired();

        // ===== FKs y snapshots =====
        b.Property(x => x.EmpresaId).IsRequired();
        b.Property(x => x.VehiculoId).IsRequired();

        b.Property(x => x.PlacaSnapshot).HasMaxLength(20);
        b.Property(x => x.ConductorId); // NULL
        b.Property(x => x.ConductorNombreSnapshot).HasMaxLength(240);

        b.Property(x => x.ClienteId);   // NULL
        b.Property(x => x.MaterialId).IsRequired();

        // ===== tinyint / estados =====
        b.Property(x => x.DestinoTipo)
            .HasColumnType("tinyint")
            .IsRequired();

        // Ya es byte → no necesita HasConversion
        b.Property(x => x.Estado)
            .HasColumnType("tinyint")
            .IsRequired();

        // ===== Otros nvarchar =====
        b.Property(x => x.UsuarioCreador)
            .HasMaxLength(128)
            .IsRequired();

        b.Property(x => x.Observaciones)
            .HasMaxLength(1024);

        b.Property(x => x.UltimaActualizacion);

        b.Property(x => x.ReciboFisicoNumero).HasMaxLength(100);
        b.Property(x => x.ReciboFisicoNumeroNorm).HasMaxLength(100);
        b.Property(x => x.NumeroGenerado).HasMaxLength(100);

        b.Property(x => x.IdempotencyKey).HasMaxLength(128);

        // ===== Bits con defaults =====
        b.Property(x => x.AutoGenerado)
            .HasDefaultValue(false)
            .IsRequired();

        b.Property(x => x.AutoGeneradoEn);

        b.Property(x => x.Activo)
            .HasDefaultValue(true)
            .IsRequired();

        // ===== Decimal exacto y NOT NULL =====
        b.Property(x => x.Cantidad)
            .HasPrecision(18, 3)
            .IsRequired();

        b.Property(x => x.AlmacenOrigenId).IsRequired();

        // ===== Índices de consulta frecuentes =====
        b.HasIndex(x => x.IdempotencyKey);
        b.HasIndex(x => x.FechaCreacion);
        b.HasIndex(x => x.Estado);
        b.HasIndex(x => x.ClienteId);
        b.HasIndex(x => x.VehiculoId);
        b.HasIndex(x => x.MaterialId);
        b.HasIndex(x => new { x.EmpresaId, x.Estado, x.FechaCreacion });
    }
}
