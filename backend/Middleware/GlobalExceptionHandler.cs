using System.Net;
using System.Text.Json;

namespace backend.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Se produjo una excepción no controlada.");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
    HttpContext context,
    Exception ex)
{
    var status = StatusCodes.Status500InternalServerError;
    var title = "Error interno";
    var detail = "Se produjo un error interno en el servidor.";
    var codigo = "ERROR_INTERNO";

    switch (ex)
    {
        case UnauthorizedAccessException:
            status = StatusCodes.Status403Forbidden;
            title = "Operación no permitida";
            detail = ex.Message;
            codigo = ex.Message;
            break;

        case InvalidOperationException:
            status = StatusCodes.Status409Conflict;
            title = "Transición inválida";
            detail = ex.Message;
            codigo = ex.Message;
            break;

        case ArgumentException:
            status = StatusCodes.Status422UnprocessableEntity;
            title = "Error de validación";
            detail = ex.Message;
            codigo = ex.Message;
            break;

        case KeyNotFoundException:
            status = StatusCodes.Status404NotFound;
            title = "Recurso no encontrado";
            detail = ex.Message;
            codigo = "RECURSO_NO_ENCONTRADO";
            break;
    }

    context.Response.StatusCode = status;
    context.Response.ContentType = "application/problem+json";

    var response = new
    {
        type = $"https://mesasitec.local/errores/{codigo.ToLower()}",
        title,
        status,
        detail,
        codigo
    };

    await context.Response.WriteAsync(
        JsonSerializer.Serialize(response));
}
}