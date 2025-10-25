//Planta.Data/Context/TransporteReadDbContext.cs V1
using System;
using Microsoft.EntityFrameworkCore;

namespace Planta.Data.Context
{
    /// DbContext de SOLO LECTURA para tablas del esquema tpt (transporte).
    public sealed class TransporteReadDbContext : DbContext
    {
        public TransporteReadDbContext(DbContextOptions<TransporteReadDbContext> options) : base(options) { }

        public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
        public DbSet<Conductor> Conductores => Set<Conductor>();
        public DbSet<VehiculoConductorHist> VehiculoConductorHist => Set<VehiculoConductorHist>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // tpt.Vehiculo
            b.Entity<Vehiculo>(e =>
            {
                e.ToTable("Vehiculo", "tpt");
                e.HasKey(x => x.Id);
                e.Property(x => x.Placa).HasMaxLength(20);
            });

            // tpt.Conductor
            b.Entity<Conductor>(e =>
            {
                e.ToTable("Conductor", "tpt");
                e.HasKey(x => x.Id);
                e.Property(x => x.Documento).HasMaxLength(64);
                e.Property(x => x.Nombre).HasMaxLength(240);
            });

            // tpt.VehiculoConductorHist
            b.Entity<VehiculoConductorHist>(e =>
            {
                e.ToTable("VehiculoConductorHist", "tpt");
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.VehiculoId, x.Hasta, x.Desde });
            });
        }
    }

    // ===== Entidades mínimas (read-only) =====
    public sealed class Vehiculo
    {
        public int Id { get; set; }
        public string? Placa { get; set; }
        public bool Activo { get; set; }
    }

    public sealed class Conductor
    {
        public int Id { get; set; }
        public string? Documento { get; set; }
        public string? Nombre { get; set; }
        public bool Activo { get; set; }
    }

    public sealed class VehiculoConductorHist
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }
        public int ConductorId { get; set; }
        public DateTimeOffset Desde { get; set; }
        public DateTimeOffset? Hasta { get; set; } // null = vigente
    }
}
