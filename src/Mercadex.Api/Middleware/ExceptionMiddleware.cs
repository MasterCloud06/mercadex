using System.Net;
using System.Text.Json;

namespace Mercadex.Api.Middleware;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;


    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
        catch (ArgumentException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.BadRequest,
                ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.Conflict,
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error no controlado");

            await HandleExceptionAsync(
                context,
                HttpStatusCode.InternalServerError,
                "Ocurrió un error interno.");
        }
    }


    private static async Task HandleExceptionAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string mensaje)
    {
        context.Response.ContentType =
            "application/json";

        context.Response.StatusCode =
            (int)statusCode;


        var respuesta = new
        {
            mensaje
        };


        await context.Response.WriteAsync(
            JsonSerializer.Serialize(respuesta));
    }
}