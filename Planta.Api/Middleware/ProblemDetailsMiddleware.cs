// Ruta: /Planta.Api/Middleware/ProblemDetailsMiddleware.cs | V1.0
#nullable enable
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Planta.Api;

public sealed class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;

    public ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (ValidationException vex)
        {
            await Write(ctx, StatusCodes.Status400BadRequest, "Validation failed",
                string.Join("; ", vex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
        }
        catch (ArgumentException aex)
        {
            await Write(ctx, StatusCodes.Status400BadRequest, "Invalid argument", aex.Message);
        }
        catch (InvalidOperationException ioex)
        {
            await Write(ctx, StatusCodes.Status409Conflict, "Operation invalid", ioex.Message);
        }
        catch (KeyNotFoundException knf)
        {
            await Write(ctx, StatusCodes.Status404NotFound, "Not found", knf.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled");
            await Write(ctx, StatusCodes.Status500InternalServerError, "Unexpected error", "Contact support.");
        }
    }

    private static Task Write(HttpContext ctx, int status, string title, string? detail = null)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";
        var pd = new ProblemDetails { Status = status, Title = title, Detail = detail, Instance = ctx.Request.Path };
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(pd));
    }
}
