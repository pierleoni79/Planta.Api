// Ruta: /Planta.Domain/Catalogo/Producto.cs | V1.0
#nullable enable
namespace Planta.Domain.Catalogo;

using Planta.Domain.Common;

public sealed class Producto : Entity<int>
{
    public string Nombre { get; private set; }
    public string? Codigo { get; private set; }
    public int? UnidadId { get; private set; }
    public bool Activo { get; private set; }

    private Producto() : base(0) { Nombre = string.Empty; }
    public Producto(int id, string nombre, string? codigo, int? unidadId, bool activo) : base(id)
    {
        Guard.AgainstNullOrWhiteSpace(nombre, nameof(nombre));
        Id = id; Nombre = nombre.Trim(); Codigo = codigo?.Trim(); UnidadId = unidadId; Activo = activo;
    }

    public void Renombrar(string nombre)
    { Guard.AgainstNullOrWhiteSpace(nombre, nameof(nombre)); Nombre = nombre.Trim(); }

    public void CambiarEstado(bool activo) => Activo = activo;
}
