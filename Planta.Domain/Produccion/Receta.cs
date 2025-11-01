// Ruta: /Planta.Domain/Produccion/Receta.cs | V1.0
#nullable enable
namespace Planta.Domain.Produccion;

using Planta.Domain.Common;

public sealed class Receta : AggregateRoot<int>
{
    public string Nombre { get; private set; }
    private readonly List<RecetaDet> _detalles = new();
    public IReadOnlyCollection<RecetaDet> Detalles => _detalles;

    public Receta(int id, string nombre) : base(id)
    { Guard.AgainstNullOrWhiteSpace(nombre, nameof(nombre)); Nombre = nombre.Trim(); }

    public void AgregarDetalle(int productoId, decimal proporcion)
    {
        Guard.AgainstNonPositive(proporcion, nameof(proporcion));
        _detalles.Add(new RecetaDet(0, this.Id, productoId, proporcion));
    }
}

public sealed class RecetaDet : Entity<int>
{
    public int RecetaId { get; }
    public int ProductoId { get; }
    public decimal Proporcion { get; }
    public RecetaDet(int id, int recetaId, int productoId, decimal proporcion) : base(id)
    { RecetaId = recetaId; ProductoId = productoId; Proporcion = proporcion; }
}
