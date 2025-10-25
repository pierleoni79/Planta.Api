// Ruta: /Planta.Api/Program.cs | V1.15
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Planta.Api.Health;
using Planta.Api.Middlewares;
using Planta.Application;                 // AssemblyMarker (scan MediatR/FluentValidation)
using Planta.Application.Abstractions;
using Planta.Data.Context;               // ✅ DbContexts reales (PlantaDbContext / TransporteReadDbContext)
using Planta.Infrastructure.Options;
// Repos/Servicios
using Planta.Infrastructure.Repositories;
using Planta.Infrastructure.Services;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// 1) Opciones (desde configuración segura)
builder.Services.Configure<ConnectionStringsOptions>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<StartupOptions>(builder.Configuration.GetSection("Startup"));
builder.Services.Configure<RecibosOptions>(builder.Configuration.GetSection("Recibos"));

// 2) DbContexts (usar los de Planta.Data.Context)
builder.Services.AddDbContext<PlantaDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("PlantaDb");
    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException("Falta ConnectionStrings:PlantaDb (usar User Secrets/Variables).");

    options.UseSqlServer(conn);
});

// Factory de SOLO LECTURA (usada por RecibosService/Catálogos)
builder.Services.AddDbContextFactory<TransporteReadDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("PlantaDb");
    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException("Falta ConnectionStrings:PlantaDb (usar User Secrets/Variables).");

    options.UseSqlServer(conn);
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// ❌ Quitar el bridge a IPlantaDbContext (no se usa en esta alineación)
// builder.Services.AddScoped<IPlantaDbContext>(sp => sp.GetRequiredService<PlantaDbContext>());

// 3) MediatR + FluentValidation (escanea Planta.Application)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(AssemblyMarker).Assembly);

// 4) Servicios de API
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5) CORS simple (ajústalo a tu dominio)
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Default", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// 6) Health checks
builder.Services.AddHealthChecks()
    .AddCheck<DbHealthCheck>("database");

// 7) Repositorios / Servicios de aplicación
builder.Services.AddScoped<ICatalogoRepository, CatalogoRepository>();
builder.Services.AddScoped<IRecibosService, RecibosService>(); // RecibosService depende de Planta.Data.Context.*

// 8) Infra transversal
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching(o =>
{
    o.UseCaseSensitivePaths = false;
    o.MaximumBodySize = 1024 * 1024; // 1 MB
});

var app = builder.Build();

// (Opcional) Migrations/Seed según appsettings:Startup
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var startup = sp.GetRequiredService<IOptions<StartupOptions>>().Value;

    if (startup.RunMigrations)
    {
        var db = sp.GetRequiredService<PlantaDbContext>();
        // db.Database.Migrate(); // si usas migraciones reales
        db.Database.EnsureCreated(); // dev/simple
    }

    if (startup.RunSeed)
    {
        // TODO: seeder si aplica
    }
}

// -------- Pipeline --------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();

// IMPORTANTE: CORS antes que ResponseCaching
app.UseCors("Default");
app.UseResponseCaching();

// Middlewares base
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthorization();

// /healthz → JSON compacto { status, checks[] }
static Task WriteHealth(HttpContext ctx, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport rpt)
{
    ctx.Response.ContentType = "application/json; charset=utf-8";
    var payload = new
    {
        status = rpt.Status.ToString(),
        checks = rpt.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            desc = e.Value.Description
        })
    };
    return ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
}

app.MapControllers();

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = WriteHealth
});

app.Run();

// Visible para pruebas de integración (WebApplicationFactory)
public partial class Program { }
