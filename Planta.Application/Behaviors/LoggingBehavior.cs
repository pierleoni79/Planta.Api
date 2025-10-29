﻿// Ruta: /Planta.Application/Behaviors/LoggingBehavior.cs | V1.0
#nullable enable
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Planta.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("→ Handling {Request}", name);
            var response = await next();
            sw.Stop();
            _logger.LogInformation("← Handled {Request} in {Elapsed} ms", name, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "✖ Error handling {Request} after {Elapsed} ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
