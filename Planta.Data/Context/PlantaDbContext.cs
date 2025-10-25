// Ruta: /Planta.Data.Context/PlantaDbContext.cs | V1.7
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Planta.Data.Entities;
using IPlantaDbContext = Planta.Application.Abstractions.IPlantaDbContext;

namespace Planta.Data.Context;

public sealed class PlantaDbContext : DbContext, IPlantaDbContext
{
    public PlantaDbContext(DbContextOptions<PlantaDbContext> options) : base(options) { }

    // DbSets usados por la API/servicios
    public DbSet<Recibo> Recibos => Set<Recibo>();
    public DbSet<ReciboEstadoLog> ReciboEstadoLogs => Set<ReciboEstadoLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Carga todas las IEntityTypeConfiguration<> del ensamblado Planta.Data
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlantaDbContext).Assembly);

        // Si la POCO Recibo TIENE 'Unidad' pero no existe en BD, descomenta:
        // modelBuilder.Entity<Recibo>().Ignore(x => x.Unidad);

        base.OnModelCreating(modelBuilder);
    }

    // ===== Implementación EXPLÍCITA de IPlantaDbContext (no tapa miembros de DbContext) =====
    IQueryable<T> IPlantaDbContext.Query<T>() where T : class
        => Set<T>().AsQueryable();

    Task IPlantaDbContext.AddAsync<T>(T entity, CancellationToken cancellationToken)
        => Set<T>().AddAsync(entity, cancellationToken).AsTask();

    void IPlantaDbContext.Update<T>(T entity) => Set<T>().Update(entity);

    void IPlantaDbContext.Remove<T>(T entity) => Set<T>().Remove(entity);

    Task<int> IPlantaDbContext.SaveChangesAsync(CancellationToken cancellationToken)
        => base.SaveChangesAsync(cancellationToken);
}
