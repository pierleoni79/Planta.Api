// Ruta: /Planta.Api/Program.cs | V1.8-fix (AddPlantaData + ReadStore + repo transporte)
#nullable enable
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Planta.Application;
using Planta.Infrastructure;
using Planta.Data.ReadStores;                         // ✅ ReadStore
using Planta.Application.Transporte.Abstractions;    // ✅ ITransporteRepository
using Planta.Data.Repositories;                      // ✅ TransporteRepository

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// 1) CORS
var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(o => o.AddPolicy("default", p =>
{
    if (origins.Length == 0)
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    else
        p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));

// 2) MVC + Versionado
builder.Services.AddControllers();
builder.Services
    .AddApiVersioning(opts =>
    {
        opts.AssumeDefaultVersionWhenUnspecified = true;
        opts.DefaultApiVersion = new ApiVersion(1, 0);
        opts.ReportApiVersions = true;
    })
    .AddApiExplorer(opts =>
    {
        opts.GroupNameFormat = "'v'VVV";
        opts.SubstituteApiVersionInUrl = true;
    });

// 3) Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var title = config["Swagger:Title"] ?? "Planta API";
    var desc = config["Swagger:Description"] ?? "";
    c.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1", Description = desc });
});

// 4) Capas internas (DI)
var cnn =
    config.GetConnectionString("PlantaDb") ??
    config.GetConnectionString("SqlServer") ??
    throw new InvalidOperationException("Falta ConnectionStrings:PlantaDb en appsettings.json.");

Planta.Data.DependencyInjection.AddPlantaData(builder.Services, cnn); // DbContext + ISqlConnectionFactory
builder.Services.AddApplication();
builder.Services.AddInfrastructure(config);

// 4.1) ReadStores y repos específicos de Transporte
builder.Services.AddScoped<TransporteReadStore>();                         // ✅ consultas (placa/favoritos)
builder.Services.AddScoped<ITransporteRepository, TransporteRepository>(); // ✅ writes auxiliares (históricos)

// 5) Response Caching (ETag/manual)
builder.Services.AddResponseCaching();

var app = builder.Build();

// 6) Migraciones opcionales (solo DbContext)
if (config.GetValue<bool>("Startup:RunMigrations"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<Planta.Data.PlantaDbContext>();
    db.Database.Migrate();
}

// -------------------- HTTP Pipeline --------------------
app.UseCors("default");

var swaggerOn = config.GetValue<bool>("Swagger:Enabled", app.Environment.IsDevelopment());
if (swaggerOn)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// En Development no forzamos HTTPS (evita 307 en emuladores)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Auth opcional
if (!config.GetValue<bool>("Auth:DisableAuth"))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseResponseCaching();

app.MapControllers();

// Raíz → Swagger
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// Healthcheck simple
app.MapGet("/health/ping", () => Results.Ok(new { ok = true, utc = DateTimeOffset.UtcNow }))
   .ExcludeFromDescription();

app.Run();
