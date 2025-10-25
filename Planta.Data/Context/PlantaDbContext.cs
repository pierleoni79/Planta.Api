// Ruta: /Planta.Data.Context/PlantaDbContext.cs | V1.4
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;   // 👈 referencia necesaria
using Planta.Data.Entities;

namespace Planta.Data.Context;

public sealed class PlantaDbContext : DbContext, IPlantaDbContext
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
