
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

    public async Task InvokeAsync(HttpContext context)
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
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "UX_socios_DocumentoNormalizado"
            })
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.Conflict,
                "Ya existe un socio registrado con ese documento de identidad.");
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "UX_obligaciones_unidad_concepto_periodo"
            })
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.Conflict,
                "La unidad ya tiene una deuda de ese concepto para ese mes y año. " +
                "Si es un cobro que se repite en el mes, marque \"Cargo eventual (sin período)\".");
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
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var respuesta = new
        {
            mensaje
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(respuesta));
    }
}
