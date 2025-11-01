// Ruta: /Planta.Application/DependencyInjection.cs | V2.0 (AutoMapper ≥13, sin paquete extra)
#nullable enable
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;            // ← aquí vive AddAutoMapper()
using Planta.Application.Common.Behaviors;

namespace Planta.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var asm = Assembly.GetExecutingAssembly();

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(asm));

        // AutoMapper (sin paquete extra):
        // escanea los Profiles del assembly actual (agrega más assemblies si necesitas).
        services.AddAutoMapper(cfg => { /* config global opcional */ }, asm);
        // (si prefieres) services.AddAutoMapper(asm);

        // FluentValidation
        services.AddValidatorsFromAssembly(asm);

        // Pipeline: Validación
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
