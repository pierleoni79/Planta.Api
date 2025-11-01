// Ruta: /Planta.Data/Configurations/ReciboEstadoLogConfig.cs | V1.1 (alineado a BD)
#nullable enable
namespace Planta.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planta.Domain.Recibos;

public sealed class ReciboEstadoLogConfig : IEntityTypeConfiguration<ReciboEstadoLog>
{
    public void Configure(EntityTypeBuilder<ReciboEstadoLog> b)
    {
        // Tabla real
        b.ToTable("ReciboEstadoLog", schema: "op");
        b.HasKey(x => x.Id);

        // Columnas
        b.Property(x => x.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        b.Property(x => x.ReciboId)
            .HasColumnName("ReciboId");

        // tinyint en BD
        b.Property(x => x.Estado)
            .HasColumnName("Estado")
            .HasConversion<byte>();

        // datetimeoffset NOT NULL
        b.Property(x => x.Cuando)
            .HasColumnName("Cuando")
            .IsRequired();

        // nvarchar(256) / nvarchar(64) NULL
        b.Property(x => x.Nota)
            .HasColumnName("Nota")
            .HasMaxLength(256);

        b.Property(x => x.Gps)
            .HasColumnName("GPS")
            .HasMaxLength(64);

        // Índices útiles
        b.HasIndex(x => x.ReciboId);
        b.HasIndex(x => x.Estado);
        b.HasIndex(x => x.Cuando);
    }
}
