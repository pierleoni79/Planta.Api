// Ruta: /Planta.Data/Repositories/IProcesoTrituracionWriteRepository.cs | V1.1
#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Planta.Domain.Produccion;

namespace Planta.Data.Repositories;

public interface IProcesoTrituracionWriteRepository
{
    /// <summary>
    /// Agrega el proceso al contexto. 
    /// Debe devolver el proceso con su Id (IDENTITY) asignado.
    /// (La implementación puede guardar inmediatamente o usar un método atómico alterno).
    /// </summary>
    Task<ProcesoTrituracion> AddProcesoAsync(ProcesoTrituracion proceso, CancellationToken ct = default);

    /// <summary>
    /// Agrega una salida al contexto asignando la propiedad sombra 'CantidadM3' (decimal(18,3)).
    /// No guarda. La implementación debe redondear a 3 decimales (AwayFromZero) antes de asignar.
    /// </summary>
    Task AddSalidaAsync(ProcesoTrituracionSalida salida, decimal cantidadM3, CancellationToken ct = default);

    /// <summary>
    /// Agrega varias salidas (todas con 'CantidadM3' como propiedad sombra).
    /// No guarda; usar SaveAsync().
    /// </summary>
    Task AddSalidasAsync(IEnumerable<(ProcesoTrituracionSalida salida, decimal cantidadM3)> salidas, CancellationToken ct = default);

    /// <summary>
    /// Operación atómica: agrega el proceso y sus salidas y persiste en un solo SaveChanges().
    /// Útil para evitar el guardado intermedio solo para obtener el Id (EF fija los FK).
    /// </summary>
    Task<ProcesoTrituracion> AddProcesoConSalidasAsync(
        ProcesoTrituracion proceso,
        IEnumerable<(int productoId, decimal cantidadM3)> salidas,
        CancellationToken ct = default);

    /// <summary>Persiste cambios pendientes.</summary>
    Task<int> SaveAsync(CancellationToken ct = default);
}
