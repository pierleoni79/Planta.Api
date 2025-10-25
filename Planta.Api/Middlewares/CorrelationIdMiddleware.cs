// Ruta: /Planta.Api/Middlewares/CorrelationIdMiddleware.cs | V1.0
namespace Planta.Api.Middlewares;

public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var cid) || string.IsNullOrWhiteSpace(cid))
        {
            cid = Guid.NewGuid().ToString("n");
            context.Request.Headers[HeaderName] = cid!;
        }

        context.Response.Headers[HeaderName] = cid!;
        context.TraceIdentifier = cid!;

        await _next(context);
    }
}
