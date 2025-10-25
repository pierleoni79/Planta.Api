// Ruta: /Planta.Application/Abstractions/IPlantaDbContext.cs | V1.0
using Microsoft.EntityFrameworkCore;
using Planta.Domain.Produccion;

namespace Planta.Application.Abstractions;

public interface IPlantaDbContext
{
    DbSet<ProcesoTrituracion> ProcesosTrituracion { get; }
    DbSet<ProcesoTrituracionSalida> ProcesosTrituracionSalidas { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
