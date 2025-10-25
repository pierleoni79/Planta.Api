using Microsoft.EntityFrameworkCore;
using Planta.Data.Entities;

namespace Planta.Data.Context;

public class PlantaDbContext : DbContext
{
    public PlantaDbContext(DbContextOptions<PlantaDbContext> options) : base(options) { }

    // DbSets usados por la API/servicios
    public DbSet<Recibo> Recibos => Set<Recibo>();
    public DbSet<ReciboEstadoLog> ReciboEstadoLogs => Set<ReciboEstadoLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cargar TODAS las IEntityTypeConfiguration<> del ensamblado Planta.Data
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlantaDbContext).Assembly);

        // Seguridad: 'Unidad' no existe en op.Recibo → no se mapea
        modelBuilder.Entity<Recibo>().Ignore(x => x.Unidad);

        base.OnModelCreating(modelBuilder);
    }
}
