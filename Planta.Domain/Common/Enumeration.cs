// Ruta: /Planta.Domain/Common/Enumeration.cs | V1.0
#nullable enable
namespace Planta.Domain.Common;

public abstract class Enumeration : IComparable
{
    public int Id { get; }
    public string Name { get; }
    protected Enumeration(int id, string name) { Id = id; Name = name; }
    public override string ToString() => Name;
    public int CompareTo(object? other) => Id.CompareTo((other as Enumeration)?.Id ?? 0);
}
