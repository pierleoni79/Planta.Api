// Ruta: /Planta.Contracts/Common/ApiResult.cs | V1.1
#nullable enable
namespace Planta.Contracts.Common;

public sealed record ApiResult<T>(
    bool Success,
    T? Data,
    string? Error = null
)
{
    public static ApiResult<T> Ok(T data) => new(true, data, null);
    public static ApiResult<T> Fail(string error) => new(false, default, error);
}
