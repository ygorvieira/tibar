using System.Text.Json;
using Tibar.Domain.Exceptions;

namespace Tibar.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Exceção de domínio: {Message}", ex.Message);

            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";

            var response = new { errors = new[] { ex.Message } };
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção não tratada");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new { errors = new[] { "Ocorreu um erro inesperado." } };
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
