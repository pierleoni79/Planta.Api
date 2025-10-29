// Ruta: /Planta.Data/Context/PlantaDbContext.cs | V2.1 (Entities-only, sin doble mapeo)
#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;
using Planta.Data.Entities; // Recibo, ReciboEstadoLog, Proceso, ProcesoDet

namespace Planta.Data.Context;

public sealed class PlantaDbContext : DbContext, IPlantaDbContext
{
    public PlantaDbContext(DbContextOptions<PlantaDbContext> options) : base(options) { }

    // === DbSets usados (Entities) ===
    public DbSet<Recibo> Recibos => Set<Recibo>();
    public DbSet<ReciboEstadoLog> ReciboEstadoLogs => Set<ReciboEstadoLog>();
    public DbSet<Proceso> Procesos => Set<Proceso>();
    public DbSet<ProcesoDet> ProcesoDets => Set<ProcesoDet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1) Aplica TODAS las configuraciones del ensamblado de Data,
        //    EXCEPTO las que pertenecen al módulo de Trituración (dominio).
        ApplyConfigurationsExceptTrituracion(modelBuilder, typeof(PlantaDbContext).Assembly);

        // 2) Blindaje extra: si por convención EF descubriera entidades de Trituración del dominio, ignóralas.
        TryIgnore(modelBuilder, "Planta.Domain.Produccion.ProcesoTrituracion");
        TryIgnore(modelBuilder, "Planta.Domain.Produccion.ProcesoTrituracionSalida");

        // 3) (Opcional) Si Recibo tuviera props no mapeadas (p.ej., Unidad), ignóralas aquí.
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
                t.GetInterfaces().Any(i => i.IsGenericType &&
                                           i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
            // EXCLUSIÓN por nombre: evita cualquier *Config de Trituración (dominio)
            .Where(t => !t.Name.Contains("Trituracion", StringComparison.OrdinalIgnoreCase));

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
