// Ruta: /Planta.Application/Common/Exceptions/ConcurrencyException.cs | V1.1
#nullable enable
namespace Planta.Application.Common.Exceptions;

public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
}
