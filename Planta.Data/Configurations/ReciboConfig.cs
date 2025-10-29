// Ruta: /Planta.Data/Configurations/ReciboConfig.cs | V1.8
#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Data.Entities;

namespace Planta.Data.Configurations;

public sealed class ReciboConfig : IEntityTypeConfiguration<Recibo>
{
    public void Configure(EntityTypeBuilder<Recibo> b)
    {
        // ===== Tabla, PK y CHECKS =====
        b.ToTable("Recibo", "op", tb =>
        {
            // 1 = Planta, 2 = ClienteDirecto
            tb.HasCheckConstraint("CK_Recibo_DestinoTipo", "[DestinoTipo] IN (1,2)");
            // Cantidad > 0
            tb.HasCheckConstraint("CK_Recibo_Cantidad_Pos", "[Cantidad] > 0");
            // Usa el nombre que ya existe en la BD para no duplicar el check
            tb.HasCheckConstraint("CK_Recibo_Estado_Valid", "[Estado] IN (10,12,20,30,40,90,99)");
        });

        b.HasKey(x => x.Id);

        // Único por empresa+consecutivo
        b.HasIndex(x => new { x.EmpresaId, x.Consecutivo }).IsUnique();

        // ===== Defaults / generación =====
        b.Property(x => x.Id).HasDefaultValueSql("newsequentialid()").ValueGeneratedOnAdd();
        b.Property(x => x.Consecutivo).HasDefaultValueSql("NEXT VALUE FOR [op].[Seq_Recibo]").ValueGeneratedOnAdd();
        b.Property(x => x.FechaCreacion).HasDefaultValueSql("sysdatetimeoffset()").ValueGeneratedOnAdd().IsRequired();

        // ===== Campos principales =====
        b.Property(x => x.EmpresaId).IsRequired();
        b.Property(x => x.VehiculoId).IsRequired();
        b.Property(x => x.MaterialId).IsRequired();
        b.Property(x => x.AlmacenOrigenId).IsRequired();

        b.Property(x => x.ClienteId);
        b.Property(x => x.ConductorId);

        b.Property(x => x.PlacaSnapshot).HasMaxLength(20);
        b.Property(x => x.ConductorNombreSnapshot).HasMaxLength(240);

        b.Property(x => x.DestinoTipo).HasColumnType("tinyint").IsRequired();
        b.Property(x => x.Estado).HasColumnType("tinyint").IsRequired();

        b.Property(x => x.UsuarioCreador).HasMaxLength(128).IsRequired();
        b.Property(x => x.Observaciones).HasMaxLength(1024);
        b.Property(x => x.UltimaActualizacion);

        b.Property(x => x.ReciboFisicoNumero).HasMaxLength(100);
        b.Property(x => x.ReciboFisicoNumeroNorm).HasMaxLength(100);
        b.Property(x => x.NumeroGenerado).HasMaxLength(100);
        b.Property(x => x.IdempotencyKey).HasMaxLength(128);

        b.Property(x => x.AutoGenerado).HasDefaultValue(false).IsRequired();
        b.Property(x => x.AutoGeneradoEn);
        b.Property(x => x.Activo).HasDefaultValue(true).IsRequired();

        b.Property(x => x.Cantidad).HasPrecision(18, 3).IsRequired();

        // ===== Índices (alineados con tu SQL) =====
        b.HasIndex(x => x.FechaCreacion).HasDatabaseName("IX_Recibo_Fecha");
        b.HasIndex(x => new { x.Estado, x.FechaCreacion }).HasDatabaseName("IX_Recibo_Estado_Fecha");
        b.HasIndex(x => new { x.DestinoTipo, x.Estado }).HasDatabaseName("IX_op_Recibo_Destino_Estado");
        b.HasIndex(x => new { x.PlacaSnapshot, x.FechaCreacion }).HasDatabaseName("IX_op_Recibo_Placa_Fecha");
        b.HasIndex(x => new { x.ClienteId, x.FechaCreacion }).HasDatabaseName("IX_op_Recibo_Cliente_Fecha");

        // Índices adicionales (si decides aplicarlos por migración)
        b.HasIndex(x => new { x.EmpresaId, x.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("UX_Recibo_IdemKey")
            .HasFilter("[IdempotencyKey] IS NOT NULL");

        b.HasIndex(x => new { x.EmpresaId, x.ReciboFisicoNumeroNorm })
            .IsUnique()
            .HasDatabaseName("UX_Recibo_Empresa_ReciboFisicoNumeroNorm")
            .HasFilter("[ReciboFisicoNumeroNorm] IS NOT NULL");

        b.HasIndex(x => new { x.EmpresaId, x.NumeroGenerado })
            .IsUnique()
            .HasDatabaseName("UX_Recibo_Empresa_NumeroGenerado")
            .HasFilter("[NumeroGenerado] IS NOT NULL");

        b.HasIndex(x => x.VehiculoId);
        b.HasIndex(x => x.MaterialId);
    }
}
