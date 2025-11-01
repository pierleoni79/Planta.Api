// Ruta: /Planta.Mobile/Models/OptionItemInt.cs | V1.0 (nuevo)
namespace Planta.Mobile.Models;

public sealed class OptionItemInt
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public override string ToString() => Nombre;
}
