// Ruta: /Planta.Domain/Common/Guard.cs | V1.0
#nullable enable
namespace Planta.Domain.Common;

public static class Guard
{
    public static void AgainstNull(object? value, string name)
    { if (value is null) throw new ArgumentNullException(name); }

    public static void AgainstNullOrWhiteSpace(string? value, string name)
    { if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{name} requerido"); }

    public static void AgainstNonPositive(decimal value, string name)
    { if (value <= 0) throw new ArgumentOutOfRangeException(name, $"{name} debe ser > 0"); }
}
