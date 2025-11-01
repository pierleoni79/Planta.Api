// Ruta: /Planta.Domain/ValueObjects/ETag.cs | V1.0
#nullable enable
namespace Planta.Domain.ValueObjects;

public readonly struct ETag
{
    public string Value { get; }
    private ETag(string value) => Value = value;
    public static ETag Weak(string token) => new($"W/\"{token}\"");
    public override string ToString() => Value;
}
