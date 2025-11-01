// Ruta: /Planta.Domain/ValueObjects/Unidad.cs | V1.0
#nullable enable
namespace Planta.Domain.ValueObjects;

public readonly record struct Unidad(string Codigo)
{
    public static Unidad From(string? codigo) =>
        new(string.IsNullOrWhiteSpace(codigo)
            ? throw new ArgumentException("Unidad vacía.")
            : codigo.Trim().ToUpperInvariant());

    public override string ToString() => Codigo;
}
