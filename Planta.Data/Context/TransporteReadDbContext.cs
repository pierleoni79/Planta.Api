// Ruta: /Planta.Data/Context/TransporteReadDbContext.cs | V1.5
#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Planta.Data.Context
{
    /// <summary>
    /// DbContext de SOLO LECTURA para el esquema tpt (transporte).
    /// - Tracking deshabilitado (NoTracking).
    /// - TODAS las variantes de SaveChanges lanzan excepción para evitar escrituras.
    /// - Entidades excluidas de migraciones (no se scaffoldéan cambios sobre estas tablas).
    /// </summary>
    public sealed class TransporteReadDbContext : DbContext
    {
        public TransporteReadDbContext(DbContextOptions<TransporteReadDbContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
        public DbSet<Conductor> Conductores => Set<Conductor>();
        public DbSet<VehiculoConductorHist> VehiculoConductorHist => Set<VehiculoConductorHist>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // ===== tpt.Vehiculo =====
            b.Entity<Vehiculo>(e =>
            {
                e.ToTable("Vehiculo", "tpt", tb => tb.ExcludeFromMigrations());
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).ValueGeneratedOnAdd();        // int IDENTITY
                e.Property(x => x.Placa).HasMaxLength(20).IsRequired();
                e.Property(x => x.Activo).IsRequired();

                e.HasIndex(x => x.Placa).HasDatabaseName("IX_tpt_Vehiculo_Placa");
                e.HasIndex(x => x.Activo).HasDatabaseName("IX_tpt_Vehiculo_Activo");
            });

            // ===== tpt.Conductor =====
            b.Entity<Conductor>(e =>
            {
                e.ToTable("Conductor", "tpt", tb => tb.ExcludeFromMigrations());
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).ValueGeneratedOnAdd();        // int IDENTITY
                e.Property(x => x.Documento).HasMaxLength(64);
                e.Property(x => x.Nombre).HasMaxLength(240);
                e.Property(x => x.Activo).IsRequired();

                e.HasIndex(x => x.Documento).HasDatabaseName("IX_tpt_Conductor_Documento");
                e.HasIndex(x => x.Activo).HasDatabaseName("IX_tpt_Conductor_Activo");
            });

            // ===== tpt.VehiculoConductorHist =====
            b.Entity<VehiculoConductorHist>(e =>
            {
                e.ToTable("VehiculoConductorHist", "tpt", tb => tb.ExcludeFromMigrations());
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).ValueGeneratedOnAdd();        // int IDENTITY
                e.Property(x => x.VehiculoId).IsRequired();
                e.Property(x => x.ConductorId).IsRequired();
                e.Property(x => x.Desde)
                    .HasDefaultValueSql("sysdatetimeoffset()")
                    .IsRequired();
                e.Property(x => x.Hasta).IsRequired(false);         // null = vigente

                // FKs explícitas (sin navegación declarada)
                e.HasOne<Vehiculo>()
                    .WithMany()
                    .HasForeignKey(x => x.VehiculoId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Conductor>()
                    .WithMany()
                    .HasForeignKey(x => x.ConductorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Búsquedas por vigencia
                e.HasIndex(x => new { x.VehiculoId, x.Hasta, x.Desde })
                 .HasDatabaseName("IX_tpt_VehConductorHist_Veh_Hasta_Desde");
            });
        }

        // ===== Blindaje SOLO LECTURA (todas las variantes) =====
        public override int SaveChanges()
            => throw new InvalidOperationException("TransporteReadDbContext es de solo lectura.");

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
            => throw new InvalidOperationException("TransporteReadDbContext es de solo lectura.");

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("TransporteReadDbContext es de solo lectura.");

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("TransporteReadDbContext es de solo lectura.");
    }

    // ===== Entidades mínimas (read-only) =====
    public sealed class Vehiculo
    {
        public int Id { get; set; }
        public string? Placa { get; set; }      // nvarchar(20) NOT NULL
        public bool Activo { get; set; }        // bit NOT NULL
        // En BD existen: ClaseVehiculoId, EmpresaId, EsFavorito → omitidos (solo lectura mínima).
    }

    public sealed class Conductor
    {
        public int Id { get; set; }
        public string? Documento { get; set; }  // nvarchar(64) NULL
        public string? Nombre { get; set; }     // nvarchar(240) NULL
        public bool Activo { get; set; }        // bit NOT NULL
    }

    public sealed class VehiculoConductorHist
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }
        public int ConductorId { get; set; }
        public DateTimeOffset Desde { get; set; }       // default sysdatetimeoffset()
        public DateTimeOffset? Hasta { get; set; }      // null = vigente
    }
}
