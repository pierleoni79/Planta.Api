// Ruta: /Planta.Application/Abstractions/IPlantaDbContext.cs | V1.1
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Planta.Domain.Produccion;

namespace Planta.Application.Abstractions;

public interface IPlantaDbContext
{
    // Acceso genérico (útil para otros módulos: check-in, catálogos, etc.)
    DbSet<T> Set<T>() where T : class;

    // Módulo D — Trituración
    DbSet<ProcesoTrituracion> ProcesosTrituracion { get; }
    DbSet<ProcesoTrituracionSalida> ProcesosTrituracionSalidas { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
