// Ruta: /Planta.Domain/Common/IAuditable.cs | V2.0 (alineado a datetimeoffset)
#nullable enable
namespace Planta.Domain.Common;

public interface IAuditable
{
    DateTimeOffset FechaCreacionUtc { get; }
    DateTimeOffset? UltimaActualizacionUtc { get; }
}
