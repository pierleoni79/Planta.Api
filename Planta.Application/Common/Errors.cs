// Ruta: /Planta.Application/Common/Errors.cs | V1.0
#nullable enable
namespace Planta.Application.Common;

public class ConflictException : Exception
{
    public string Code { get; }
    public ConflictException(string code, string message) : base(message) => Code = code;
}
public class PreconditionFailedException : Exception
{
    public string Code { get; }
    public PreconditionFailedException(string code, string message) : base(message) => Code = code;
}
public class ValidationAppException : Exception
{
    public string Code { get; }
    public ValidationAppException(string code, string message) : base(message) => Code = code;
}
