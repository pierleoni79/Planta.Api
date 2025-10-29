// Ruta: /Planta.Data/Configurations/ProcesoTrituracionConfig.cs | V2.6
#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Domain.Produccion;
using Planta.Data.Entities; // FK → op.Recibo

namespace Planta.Data.Configurations;

public sealed class ProcesoTrituracionConfig : IEntityTypeConfiguration<ProcesoTrituracion>
{
    public void Configure(EntityTypeBuilder<ProcesoTrituracion> b)
    {
        // ===== Tabla, PK y CHECKS =====
        b.ToTable("Proceso", "prd", tb =>
        {
            // Asegura que la entrada sea positiva (columna real en BD)
            tb.HasCheckConstraint("CK_Proceso_EntradaM3_Pos", "[EntradaM3] > 0");
        });

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .ValueGeneratedOnAdd(); // int IDENTITY

        // ===== Claves / columnas =====
        b.Property(x => x.ReciboId).IsRequired();

        // EntradaM3 (DB) ← PesoEntrada (Dominio)
        b.Property(x => x.PesoEntrada)
            .HasColumnName("EntradaM3")
            .HasPrecision(18, 3)
            .IsRequired();

        // Sombras que existen en la tabla pero no en el dominio
        b.Property<int?>("RecetaId")
            .HasColumnName("RecetaId")
            .IsRequired(false);

        b.Property<byte>("Estado")
            .HasColumnName("Estado")
            .HasColumnType("tinyint")
            .HasDefaultValue((byte)0);

        b.Property<string?>("Observacion")
            .HasColumnName("Observacion")
            .HasMaxLength(1024)
            .IsRequired(false);

        // CreadoEn (DB) ← CreatedAtUtc (Dominio)
        b.Property(x => x.CreatedAtUtc)
            .HasColumnName("CreadoEn")
            .HasDefaultValueSql("sysdatetimeoffset()")
            .ValueGeneratedOnAdd();

        // ===== Índices (alineados con tu SQL) =====
        // Coincide con: IX_prd_Proceso_Recibo_Estado_CreadoEn
        b.HasIndex(x => new { x.ReciboId, /* sombra */ Estado = EF.Property<byte>(x, "Estado"), x.CreatedAtUtc })
            .HasDatabaseName("IX_prd_Proceso_Recibo_Estado_CreadoEn");

        // ===== Relaciones =====
        b.HasMany(x => x.Salidas)
            .WithOne(s => s.Proceso!)
            .HasForeignKey(s => s.ProcesoId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK explícita a op.Recibo SIN cascada (preserva histórico)
        b.HasOne<Recibo>()
            .WithMany()
            .HasForeignKey(x => x.ReciboId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Propiedades solo de dominio (no persistir) =====
        b.Ignore(x => x.PesoSalida);
        b.Ignore(x => x.Residuos);
        b.Ignore(x => x.BalancePorc);
        b.Ignore(x => x.Epsilon);
        b.Ignore(x => x.DeltaMin);
    }
}
