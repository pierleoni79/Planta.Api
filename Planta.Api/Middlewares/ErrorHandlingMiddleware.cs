// Ruta: /Planta.Api/Middlewares/ErrorHandlingMiddleware.cs | V1.1
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http; // <-- necesario para StatusCodes

namespace Planta.Api.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Cancelación del cliente: no loggear como error del servidor
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest; // 499
            }
            // No escribimos body para 499
        }
        catch (Exception ex)
        {
            var cid = context.TraceIdentifier;
            _logger.LogError(ex, "Unhandled exception. cid={CorrelationId}", cid);

            if (context.Response.HasStarted)
            {
                // Si ya empezó la respuesta, no podemos cambiar cabeceras/código
                throw;
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var problem = new
            {
                type = "https://httpstatuses.io/500",
                title = "Error interno del servidor",
                status = 500,
                traceId = cid
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
