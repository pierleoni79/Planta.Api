// Ruta: /Planta.Domain/Produccion/ProcesoTrituracionSalida.cs | V1.0
namespace Planta.Domain.Produccion;

public sealed class ProcesoTrituracionSalida
{
    public int Id { get; set; }
    public Guid ProcesoId { get; set; }
    public int ProductoId { get; set; }
    public double Cantidad { get; set; }
    public bool EsMerma { get; set; }
    public bool Vendible { get; set; }

    public ProcesoTrituracion? Proceso { get; set; }
}
