using System.Net;
using System.Text.Json;

namespace Mercadex.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;


    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }


    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, ex.Message);

            context.Response.StatusCode =
                (int)HttpStatusCode.BadRequest;

            context.Response.ContentType =
                "application/json";


            var response = new
            {
                mensaje = ex.Message
            };


            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;

            context.Response.ContentType =
                "application/json";


            var response = new
            {
                mensaje = "Ocurrió un error interno."
            };


            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}