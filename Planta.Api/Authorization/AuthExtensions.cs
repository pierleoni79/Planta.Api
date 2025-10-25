// Ruta: /Planta.Api/Authorization/AuthExtensions.cs | V1.0
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Planta.Api.Authorization;

public static class AuthExtensions
{
    public static IServiceCollection AddAuthAndPolicies(
        this IServiceCollection services,
        IConfiguration config,
        bool disableAuthForDev)
    {
        if (!disableAuthForDev)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        // Configura tu emisor/validador real
                        options.Authority = config["Auth:Jwt:Authority"];   // ej. https://login.microsoftonline.com/...
                        options.Audience = config["Auth:Jwt:Audience"];    // ej. planta.api
                        options.RequireHttpsMetadata = true;
                    });
        }

        services.AddAuthorization(opt =>
        {
            // Permite 2 caminos: scope O role
            var catalogRequirement = new AuthorizationPolicyBuilder()
                .RequireAssertion(ctx =>
                    disableAuthForDev // Bypass en Dev si así lo decides
                    || ctx.User.HasClaim("scope", "catalog.read")
                    || ctx.User.IsInRole("CatalogReader")
                )
                .Build();

            opt.AddPolicy(Policies.CatalogRead, catalogRequirement);
        });

        return services;
    }
}
