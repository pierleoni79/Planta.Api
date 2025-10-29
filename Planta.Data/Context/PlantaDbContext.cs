// Ruta: /Planta.Data/Context/PlantaDbContext.cs | V2.0 (domain-centric, sin doble mapeo)
#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;
using Planta.Data.Entities;                  // Recibo, ReciboEstadoLog
using Planta.Domain.Produccion;              // ProcesoTrituracion, ProcesoTrituracionSalida

namespace Planta.Data.Context;

public sealed class PlantaDbContext : DbContext, IPlantaDbContext
{
    public PlantaDbContext(DbContextOptions<PlantaDbContext> options) : base(options) { }

    // === DbSets usados ===
    public DbSet<Recibo> Recibos => Set<Recibo>();
    public DbSet<ReciboEstadoLog> ReciboEstadoLogs => Set<ReciboEstadoLog>();

    // Dominio (Trituración) — ¡OJO! No mezclar con Entities.Proceso/ProcesoDet
    public DbSet<ProcesoTrituracion> Procesos => Set<ProcesoTrituracion>();
    public DbSet<ProcesoTrituracionSalida> ProcesoSalidas => Set<ProcesoTrituracionSalida>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1) Aplica todas las configuraciones del ensamblado Data,
        //    EXCEPTO las legacy de Entities.Proceso/ProcesoDet (evita doble mapeo).
        ApplyConfigurationsExceptLegacyProceso(modelBuilder, typeof(PlantaDbContext).Assembly);

        // 2) Blindaje: si aún existen las classes legacy en el proyecto, ignóralas explícitamente.
        TryIgnore(modelBuilder, "Planta.Data.Entities.Proceso");
        TryIgnore(modelBuilder, "Planta.Data.Entities.ProcesoDet");

        // Si Recibo tuviera propiedades no mapeadas (p.ej. Unidad), puedes ignorarlas aquí.
        // modelBuilder.Entity<Recibo>().Ignore(x => x.Unidad);

        base.OnModelCreating(modelBuilder);
    }

    private static void ApplyConfigurationsExceptLegacyProceso(ModelBuilder mb, Assembly asm)
    {
        var cfgTypes = asm
            .GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                !t.IsGenericTypeDefinition &&
                t.GetInterfaces().Any(i => i.IsGenericType &&
                                           i.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>)))
            // Excluye SOLO las configs legacy de Entities (evita doble mapeo con dominio)
            .Where(t =>
                !t.Name.Equals("ProcesoConfig", StringComparison.OrdinalIgnoreCase) &&
                !t.Name.Equals("ProcesoDetConfig", StringComparison.OrdinalIgnoreCase));

        foreach (var cfg in cfgTypes)
        {
            var instance = Activator.CreateInstance(cfg)
                ?? throw new InvalidOperationException($"No se pudo instanciar {cfg.FullName}.");
            mb.ApplyConfiguration((dynamic)instance);
        }
    }

    private static void TryIgnore(ModelBuilder mb, string fullTypeName)
    {
        var t = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(a => a.GetType(fullTypeName, throwOnError: false))
            .FirstOrDefault(x => x != null);

        if (t is null) return;

        var ignoreGeneric = typeof(ModelBuilder).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == nameof(ModelBuilder.Ignore) && m.IsGenericMethod && m.GetParameters().Length == 0)
            ?? throw new MissingMethodException("No se encontró ModelBuilder.Ignore<T>().");

        ignoreGeneric.MakeGenericMethod(t).Invoke(mb, null);
    }

    // ===== IPlantaDbContext =====
    IQueryable<T> IPlantaDbContext.Query<T>() where T : class => Set<T>().AsQueryable();
    Task IPlantaDbContext.AddAsync<T>(T entity, CancellationToken ct) => Set<T>().AddAsync(entity, ct).AsTask();
    void IPlantaDbContext.Update<T>(T entity) => Set<T>().Update(entity);
    void IPlantaDbContext.Remove<T>(T entity) => Set<T>().Remove(entity);
    Task<int> IPlantaDbContext.SaveChangesAsync(CancellationToken ct) => base.SaveChangesAsync(ct);
}
