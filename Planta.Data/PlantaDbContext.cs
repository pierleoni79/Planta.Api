// Ruta: /Planta.Data/PlantaDbContext.cs | V1.1
#nullable enable
using Microsoft.EntityFrameworkCore;
using Planta.Domain.Repositories;
using Planta.Domain.Recibos;

namespace Planta.Data;

public sealed class PlantaDbContext : DbContext, IUnitOfWork
{
    public PlantaDbContext(DbContextOptions<PlantaDbContext> options) : base(options) { }

    public DbSet<Recibo> Recibos => Set<Recibo>();
    public DbSet<ReciboEstadoLog> ReciboEstadoLogs => Set<ReciboEstadoLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.ReciboConfig());
        modelBuilder.ApplyConfiguration(new Configurations.ReciboEstadoLogConfig());
        base.OnModelCreating(modelBuilder);
    }

    // ✅ Sobrescribe el método base para evitar CS0114
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        => base.SaveChangesAsync(ct);
}
