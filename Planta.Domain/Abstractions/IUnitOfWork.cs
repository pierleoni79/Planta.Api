// Ruta: /Planta.Domain/Abstractions/IUnitOfWork.cs | V1.0
#nullable enable
namespace Planta.Domain.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
