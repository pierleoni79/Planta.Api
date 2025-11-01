// Ruta: /Planta.Data/Configurations/ReciboConfig.cs | V1.2
#nullable enable
namespace Planta.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Domain.Recibos;

public sealed class ReciboConfig : IEntityTypeConfiguration<Recibo>
{
    public void Configure(EntityTypeBuilder<Recibo> b)
    {
        // Tabla real
        b.ToTable("Recibo", schema: "op");
        b.HasKey(x => x.Id);

        // -------- Columnas principales ----------
        b.Property(x => x.Id).HasColumnName("Id");

        b.Property(x => x.EmpresaId).HasColumnName("EmpresaId").IsRequired();
        b.Property(x => x.PlantaId).HasColumnName("PlantaId");
        b.Property(x => x.AlmacenOrigenId).HasColumnName("AlmacenOrigenId");
        b.Property(x => x.ClienteId).HasColumnName("ClienteId");
        b.Property(x => x.VehiculoId).HasColumnName("VehiculoId");
        b.Property(x => x.ConductorId).HasColumnName("ConductorId");
        b.Property(x => x.MaterialId).HasColumnName("MaterialId");

        // tinyint en BD, int en dominio → conversión explícita
        b.Property(x => x.Estado)
            .HasColumnName("Estado")
            .HasConversion<byte>()
            .HasColumnType("tinyint");

        b.Property(x => x.DestinoTipo)
            .HasColumnName("DestinoTipo")
            .HasConversion<byte>()
            .HasColumnType("tinyint");

        b.Property(x => x.Cantidad)
            .HasColumnName("Cantidad")
            .HasColumnType("decimal(18,3)");

        b.Property(x => x.Observaciones)
            .HasColumnName("Observaciones")
            .HasMaxLength(512);

        // -------- Snapshots / documentos / auditoría ----------
        b.Property(x => x.PlacaSnapshot)
            .HasColumnName("PlacaSnapshot")
            .HasMaxLength(10);

        b.Property(x => x.ConductorNombreSnapshot)
            .HasColumnName("ConductorNombreSnapshot")
            .HasMaxLength(120);

        b.Property(x => x.ReciboFisicoNumero)
            .HasColumnName("ReciboFisicoNumero")
            .HasMaxLength(50);

        b.Property(x => x.NumeroGenerado)
            .HasColumnName("NumeroGenerado")
            .HasMaxLength(50);

        b.Property(x => x.IdempotencyKey)
            .HasColumnName("IdempotencyKey")
            .HasMaxLength(64);

        b.Property(x => x.FechaCreacionUtc)
            .HasColumnName("FechaCreacion")
            .HasColumnType("datetimeoffset")
            .HasDefaultValueSql("sysdatetimeoffset()")
            .IsRequired();

        b.Property(x => x.UltimaActualizacionUtc)
            .HasColumnName("UltimaActualizacion")
            .HasColumnType("datetimeoffset")
            .IsRequired(false);

        // -------- Sombras que existen en la BD ----------
        b.Property<int?>("Consecutivo")
            .HasColumnName("Consecutivo")
            .HasDefaultValueSql("NEXT VALUE FOR [op].[Seq_Recibo]");

        b.Property<bool?>("Activo")
            .HasColumnName("Activo")
            .HasDefaultValue(true);

        b.Property<bool?>("AutoGenerado")
            .HasColumnName("AutoGenerado")
            .HasDefaultValue(false);

        b.Property<DateTimeOffset?>("AutoGeneradoEn")
            .HasColumnName("AutoGeneradoEn")
            .HasColumnType("datetimeoffset");

        b.Property<string?>("UsuarioCreador")
            .HasColumnName("UsuarioCreador")
            .HasMaxLength(64);

        b.Property<string?>("ReciboFisicoNumeroNorm")
            .HasColumnName("ReciboFisicoNumeroNorm")
            .HasMaxLength(50);

        // -------- Relación + backing field (_historial) ----------
        b.HasMany(r => r.Historial)
         .WithOne()
         .HasForeignKey(x => x.ReciboId)
         .OnDelete(DeleteBehavior.Cascade);

        b.Navigation(r => r.Historial)
         .HasField("_historial")
         .UsePropertyAccessMode(PropertyAccessMode.Field);

        // -------- Índices útiles ----------
        b.HasIndex(x => x.EmpresaId);
        b.HasIndex(x => x.Estado);
        b.HasIndex(x => x.DestinoTipo);
        b.HasIndex(x => x.NumeroGenerado);
        b.HasIndex(x => x.FechaCreacionUtc);
        b.HasIndex(x => new { x.EmpresaId, x.Estado, x.DestinoTipo, x.FechaCreacionUtc });
    }
}
