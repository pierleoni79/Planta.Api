// Ruta: /Planta.Application/Abstractions/IUnitOfWork.cs | V1.0
namespace Planta.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
