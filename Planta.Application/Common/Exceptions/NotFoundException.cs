// Ruta: /Planta.Application/Common/Exceptions/NotFoundException.cs | V1.1
#nullable enable
namespace Planta.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
