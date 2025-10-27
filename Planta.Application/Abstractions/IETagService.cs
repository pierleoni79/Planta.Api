#nullable enable
using Planta.Domain.Recibos;

namespace Planta.Application.Abstractions;

public interface IETagService
{
    string ComputeETag(Recibo recibo);
    bool Matches(string? ifMatchHeader, Recibo recibo);
}
