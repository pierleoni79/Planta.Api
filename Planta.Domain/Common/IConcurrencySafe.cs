// Ruta: /Planta.Domain/Common/IConcurrencySafe.cs | V1.0
#nullable enable
namespace Planta.Domain.Common;

public interface IConcurrencySafe
{
    string ComputeWeakETag();
}
