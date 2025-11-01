// Ruta: /Planta.Infrastructure/Time/SystemClock.cs | V1.1
#nullable enable
namespace Planta.Infrastructure;

/// <summary>Implementación de IClock basada en DateTime.UtcNow.</summary>
public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
