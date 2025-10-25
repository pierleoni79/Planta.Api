// Ruta: /Planta.Data/Context/PlantaDbContext.cs | V1.1
using Microsoft.EntityFrameworkCore;
using Planta.Data.Entities;

namespace Planta.Data.Context;

public class PlantaDbContext : DbContext
{
    public PlantaDbContext(DbContextOptions<PlantaDbContext> options) : base(options) { }

    public DbSet<Recibo> Recibos => Set<Recibo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recibo>(b =>
        {
            b.ToTable("Recibo", "op");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            b.Property(x => x.Consecutivo).HasDefaultValueSql("NEXT VALUE FOR [op].[Seq_Recibo]");
            b.Property(x => x.UsuarioCreador).HasMaxLength(64).IsRequired();
            b.Property(x => x.Observaciones).HasMaxLength(512);
            b.Property(x => x.PlacaSnapshot).HasMaxLength(10);
            b.Property(x => x.Unidad).HasMaxLength(10);
            b.Property(x => x.IdempotencyKey).HasMaxLength(64);
            // Índices sugeridos si usas migraciones:
            // b.HasIndex(x => new { x.EmpresaId, x.Estado, x.FechaCreacion }).HasDatabaseName("IX_Recibo_Emp_Estado_Fecha");
            // b.HasIndex(x => new { x.EmpresaId, x.Consecutivo }).IsUnique().HasDatabaseName("UQ_Recibo_Empresa_Consecutivo");
            // b.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL").HasDatabaseName("UQ_Recibo_IdempotencyKey");
        });

        base.OnModelCreating(modelBuilder);
    }
}
