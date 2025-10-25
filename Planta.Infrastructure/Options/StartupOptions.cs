// Ruta: /Planta.Infrastructure/Options/StartupOptions.cs | V1.0
namespace Planta.Infrastructure.Options;

public sealed class StartupOptions
{
    public bool SkipHost { get; set; }
    public bool RunMigrations { get; set; }
    public bool RunSeed { get; set; }
}
