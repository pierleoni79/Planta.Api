// Ruta: /Planta.Domain/Enums/DestinoTipo.cs | V1.0
#nullable enable
namespace Planta.Domain.Enums;

using Planta.Domain.Common;

public sealed class DestinoTipo : Enumeration
{
    public static readonly DestinoTipo Planta = new(1, nameof(Planta));
    public static readonly DestinoTipo ClienteDirecto = new(2, nameof(ClienteDirecto));
    private DestinoTipo(int id, string name) : base(id, name) { }
}
