// Ruta: /Planta.Contracts/Recibos/DescargaFinResult.cs | V1.0
namespace Planta.Contracts.Recibos;

public sealed class DescargaFinResult
{
    public Guid Id { get; init; }
    public int Estado { get; init; }
    public bool Updated { get; init; }
    public string ETag { get; init; } = default!;
    public bool Idempotent { get; init; }
}
