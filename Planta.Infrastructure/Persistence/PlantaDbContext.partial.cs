// Ruta: /Planta.Infrastructure/Persistence/PlantaDbContext.partial.cs | V1.0
using Microsoft.EntityFrameworkCore;
using Planta.Application.Abstractions;
using Planta.Domain.Produccion;

namespace Planta.Infrastructure.Persistence;

public partial class PlantaDbContext : DbContext, IPlantaDbContext
{
    public DbSet<ProcesoTrituracion> ProcesosTrituracion => Set<ProcesoTrituracion>();
    public DbSet<ProcesoTrituracionSalida> ProcesosTrituracionSalidas => Set<ProcesoTrituracionSalida>();
}
