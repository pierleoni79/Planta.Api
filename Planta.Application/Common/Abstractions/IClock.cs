// Ruta: /Planta.Application/Common/Abstractions/IClock.cs | V1.1
#nullable enable
namespace Planta.Application.Common.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
