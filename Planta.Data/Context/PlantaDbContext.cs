// Ruta: /Planta.Data.Context/PlantaDbContext.cs | V1.10 (ignora Trituración)
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;
using Planta.Data.Entities;
using System;
using System.Linq;
using System.Reflection;

namespace Planta.Data.Context;

public sealed class PlantaDbContext : DbContext, IPlantaDbContext
{
    public PlantaDbContext(DbContextOptions<PlantaDbContext> options) : base(options) { }

    // DbSets que sí usamos
    public DbSet<Recibo> Recibos => Set<Recibo>();
    public DbSet<ReciboEstadoLog> ReciboEstadoLogs => Set<ReciboEstadoLog>();
    public DbSet<Proceso> Procesos => Set<Proceso>();
    public DbSet<ProcesoDet> ProcesoDets => Set<ProcesoDet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1) Aplica TODAS las configuraciones del ensamblado de Data,
        //    EXCEPTO las que pertenecen al módulo de Trituración.
        ApplyConfigurationsExceptTrituracion(modelBuilder, typeof(PlantaDbContext).Assembly);

        // 2) Blindaje extra: si por convención EF llegara a descubrir
        //    entidades de Trituración, ignóralas por nombre (sin referenciarlas).
        TryIgnore(modelBuilder, "Planta.Domain.Produccion.ProcesoTrituracion");
        TryIgnore(modelBuilder, "Planta.Domain.Produccion.ProcesoTrituracionSalida");

        // Si Recibo tuviera 'Unidad' en POCO pero no en BD:
        // modelBuilder.Entity<Recibo>().Ignore(x => x.Unidad);

        base.OnModelCreating(modelBuilder);
    }

    private static void ApplyConfigurationsExceptTrituracion(ModelBuilder mb, Assembly asm)
    {
        var cfgTypes = asm
            .GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                !t.IsGenericTypeDefinition &&
                t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
            // <- EXCLUSIÓN por nombre: evita cualquier *Config de Trituración
            .Where(t => !t.Name.Contains("Trituracion", StringComparison.OrdinalIgnoreCase));

        foreach (var cfg in cfgTypes)
        {
            dynamic instance = Activator.CreateInstance(cfg)!;
            mb.ApplyConfiguration(instance);
        }
    }

    private static void TryIgnore(ModelBuilder mb, string fullTypeName)
    {
        var t = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(a => a.GetType(fullTypeName, throwOnError: false))
            .FirstOrDefault(x => x != null);

        if (t != null)
            mb.Ignore(t);
    }

    // ===== Implementación EXPLÍCITA IPlantaDbContext (como ya la tenías) =====
    IQueryable<T> IPlantaDbContext.Query<T>() where T : class => Set<T>().AsQueryable();
    Task IPlantaDbContext.AddAsync<T>(T entity, CancellationToken ct) => Set<T>().AddAsync(entity, ct).AsTask();
    void IPlantaDbContext.Update<T>(T entity) => Set<T>().Update(entity);
    void IPlantaDbContext.Remove<T>(T entity) => Set<T>().Remove(entity);
    Task<int> IPlantaDbContext.SaveChangesAsync(CancellationToken ct) => base.SaveChangesAsync(ct);
}
