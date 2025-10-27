#nullable enable
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

    // DbSets que sí usamos (Planta.Data.Entities)
    public DbSet<Recibo> Recibos => Set<Recibo>();
    public DbSet<ReciboEstadoLog> ReciboEstadoLogs => Set<ReciboEstadoLog>();
    public DbSet<Proceso> Procesos => Set<Proceso>();
    public DbSet<ProcesoDet> ProcesoDets => Set<ProcesoDet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1) Aplica TODAS las configuraciones del ensamblado de Data,
        //    EXCEPTO las que pertenecen al módulo de Trituración (*Trituracion* en el nombre).
        ApplyConfigurationsExceptTrituracion(modelBuilder, typeof(PlantaDbContext).Assembly);

        // 2) Blindaje extra: si por convención EF llegara a descubrir
        //    entidades de Trituración del dominio, ignóralas por nombre.
        TryIgnore(modelBuilder, "Planta.Domain.Produccion.ProcesoTrituracion");
        TryIgnore(modelBuilder, "Planta.Domain.Produccion.ProcesoTrituracionSalida");

        // Si Recibo tuviera 'Unidad' en POCO pero no en BD, ignórala aquí.
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
            // EXCLUSIÓN por nombre: evita cualquier *Config de Trituración
            .Where(t => !t.Name.Contains("Trituracion", StringComparison.OrdinalIgnoreCase));

        foreach (var cfg in cfgTypes)
        {
            // Instanciación segura
            var instance = Activator.CreateInstance(cfg)
                ?? throw new InvalidOperationException($"No se pudo instanciar {cfg.FullName}.");

            // ApplyConfiguration(instance) vía dynamic para respetar el genérico
            mb.ApplyConfiguration((dynamic)instance);
        }
    }

    /// <summary>
    /// Ignora un tipo conocido por nombre completo usando ModelBuilder.Ignore&lt;T&gt;().
    /// Implementado por reflexión para evitar depender de una sobrecarga no genérica.
    /// </summary>
    private static void TryIgnore(ModelBuilder mb, string fullTypeName)
    {
        var t = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(a => a.GetType(fullTypeName, throwOnError: false))
            .FirstOrDefault(x => x != null);

        if (t is null) return;

        // Busca el método genérico Ignore<T>() sin parámetros
        var ignoreGeneric = typeof(ModelBuilder).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m =>
                m.Name == nameof(ModelBuilder.Ignore) &&
                m.IsGenericMethod &&
                m.GetParameters().Length == 0);

        if (ignoreGeneric is null)
            throw new MissingMethodException("No se encontró ModelBuilder.Ignore<T>() en esta versión de EF Core.");

        ignoreGeneric.MakeGenericMethod(t).Invoke(mb, null);
    }

    // ===== Implementación explícita de IPlantaDbContext =====
    IQueryable<T> IPlantaDbContext.Query<T>() where T : class => Set<T>().AsQueryable();
    Task IPlantaDbContext.AddAsync<T>(T entity, CancellationToken ct) => Set<T>().AddAsync(entity, ct).AsTask();
    void IPlantaDbContext.Update<T>(T entity) => Set<T>().Update(entity);
    void IPlantaDbContext.Remove<T>(T entity) => Set<T>().Remove(entity);
    Task<int> IPlantaDbContext.SaveChangesAsync(CancellationToken ct) => base.SaveChangesAsync(ct);
}
