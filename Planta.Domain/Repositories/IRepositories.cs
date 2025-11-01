// Ruta: /Planta.Domain/Repositories/IRepositories.cs | V1.0
#nullable enable
namespace Planta.Domain.Repositories;

using Planta.Domain.Recibos;
using Planta.Domain.Catalogo;
using Planta.Domain.Ventas;

public interface IReciboRepository
{
    Task<Recibo?> GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Recibo entity, CancellationToken ct = default);
}

public interface IProductoRepository
{
    Task<Producto?> GetAsync(int id, CancellationToken ct = default);
}

public interface IRemisionRepository
{
    Task<Remision?> GetAsync(int id, CancellationToken ct = default);
}
