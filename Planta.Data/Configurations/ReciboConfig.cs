#nullable enable
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

        // Unicidad dentro de la empresa
        b.HasIndex(x => new { x.EmpresaId, x.Consecutivo }).IsUnique();

        // ===== Defaults y generación (según tu BD) =====
        b.Property(x => x.Id)
            .HasColumnName("Id")
            .HasDefaultValueSql("newsequentialid()")
            .ValueGeneratedOnAdd();

        b.Property(x => x.Consecutivo)
            .HasColumnName("Consecutivo")
            .HasDefaultValueSql("NEXT VALUE FOR [op].[Seq_Recibo]")
            .ValueGeneratedOnAdd();

        b.Property(x => x.FechaCreacion)
            .HasColumnName("FechaCreacion")
            .HasDefaultValueSql("sysdatetimeoffset()")
            .IsRequired();

        // ===== FK y snapshots =====
        b.Property(x => x.EmpresaId).IsRequired();
        b.Property(x => x.VehiculoId).IsRequired();

        b.Property(x => x.PlacaSnapshot)
            .HasMaxLength(20)
            .IsRequired(false);

        b.Property(x => x.ConductorId).IsRequired(false);

        b.Property(x => x.ConductorNombreSnapshot)
            .HasMaxLength(240)
            .IsRequired(false);

        b.Property(x => x.ClienteId).IsRequired(false);
        b.Property(x => x.MaterialId).IsRequired();

        // ===== tinyint / enum =====
        b.Property(x => x.DestinoTipo)
            .HasColumnName("DestinoTipo")
            .HasColumnType("tinyint")
            // .HasConversion<byte>() // <- descomenta si DestinoTipo es enum
            .IsRequired();

        b.Property(x => x.Estado)
            .HasColumnName("Estado")
            .HasColumnType("tinyint")
            .HasConversion<byte>() // enum -> tinyint
            .IsRequired();

        // ===== Otros nvarchar =====
        b.Property(x => x.UsuarioCreador)
            .HasMaxLength(128)
            .IsRequired();

        b.Property(x => x.Observaciones)
            .HasMaxLength(1024)
            .IsRequired(false);

        b.Property(x => x.UltimaActualizacion).IsRequired(false);

        b.Property(x => x.ReciboFisicoNumero)
            .HasMaxLength(100)
            .IsRequired(false);

        b.Property(x => x.ReciboFisicoNumeroNorm)
            .HasMaxLength(100)
            .IsRequired(false);

        b.Property(x => x.NumeroGenerado)
            .HasMaxLength(100)
            .IsRequired(false);

        b.Property(x => x.IdempotencyKey)
            .HasMaxLength(128)
            .IsRequired(false);

        // ===== Bits con defaults =====
        b.Property(x => x.AutoGenerado)
            .HasDefaultValue(false)
            .IsRequired();

        b.Property(x => x.AutoGeneradoEn).IsRequired(false);

        b.Property(x => x.Activo)
            .HasDefaultValue(true)
            .IsRequired();

        // ===== Decimal exacto y NOT NULL =====
        b.Property(x => x.Cantidad)
            .HasPrecision(18, 3)
            .IsRequired();

        // NOT NULL en tu tabla
        b.Property(x => x.AlmacenOrigenId).IsRequired();

        // ===== Índices de búsqueda =====
        b.HasIndex(x => x.IdempotencyKey);
        b.HasIndex(x => x.FechaCreacion);
        b.HasIndex(x => x.ClienteId);
        b.HasIndex(x => x.Estado);
    }
}
