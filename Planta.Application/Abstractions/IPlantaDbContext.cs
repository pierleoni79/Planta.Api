// Ruta: /Planta.Application/Abstractions/IPlantaDbContext.cs | V1.2
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Planta.Application.Abstractions
{
    /// Abstracción liviana para consumir DbContext desde Application sin acoplar EF.
    public interface IPlantaDbContext
    {
        IQueryable<T> Query<T>() where T : class;
        Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;
        void Update<T>(T entity) where T : class;
        void Remove<T>(T entity) where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
