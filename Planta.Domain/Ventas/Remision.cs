// Ruta: /Planta.Domain/Ventas/Remision.cs | V1.0
#nullable enable
namespace Planta.Domain.Ventas;

using Planta.Domain.Common;

public sealed class Remision : AggregateRoot<int>
{
    public int EmpresaId { get; private set; }
    public int ClienteId { get; private set; }
    public DateTime FechaUtc { get; private set; }
    private readonly List<RemisionDet> _detalles = new();
    public IReadOnlyCollection<RemisionDet> Detalles => _detalles;

    public Remision(int id, int empresaId, int clienteId, DateTime fechaUtc) : base(id)
    { EmpresaId = empresaId; ClienteId = clienteId; FechaUtc = fechaUtc; }

    public void AgregarDetalle(int productoId, decimal cantidad)
    { Guard.AgainstNonPositive(cantidad, nameof(cantidad)); _detalles.Add(new RemisionDet(0, this.Id, productoId, cantidad)); }
}

public sealed class RemisionDet : Entity<int>
{
    public int RemisionId { get; }
    public int ProductoId { get; }
    public decimal Cantidad { get; }
    public RemisionDet(int id, int remisionId, int productoId, decimal cantidad) : base(id)
    { RemisionId = remisionId; ProductoId = productoId; Cantidad = cantidad; }
}
