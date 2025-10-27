#nullable enable
namespace Planta.Domain.Shared;

public readonly struct IdempotencyKey
{
    public IdempotencyKey(string scope, string key)
    {
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        Key = key ?? throw new ArgumentNullException(nameof(key));
    }

    public string Scope { get; }
    public string Key { get; }

    public override string ToString() => $"{Scope}:{Key}";
}
