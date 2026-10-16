using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mercadex.Api.Seguridad;

/// <summary>
/// Permisos de Mercadex de escritorio según el rol:
/// <list type="bullet">
/// <item>Administrador: todo.</item>
/// <item>Cajero: consultar todo; registrar cobros, egresos y el cierre de caja. No anula.</item>
/// <item>Consulta: solo consultar.</item>
/// </list>
/// Mientras MERCADEX_EXIGIR_LOGIN no esté en true, las peticiones sin sesión
/// se siguen atendiendo (instaladores anteriores a la 1.6.2). Las que llegan
/// con sesión siempre respetan su rol.
/// </summary>
public sealed class PermisosPorRolFilter : IAuthorizationFilter
{
    /// <summary>Controladores con su propia autorización.</summary>
    private static readonly HashSet<string> ConAutorizacionPropia = new(StringComparer.Ordinal)
    {
        "Auth", "Health", "AppSocios", "PanelCaja", "Usuarios", "RegistroCambios"
    };

    /// <summary>Lo que el cajero puede registrar.</summary>
    private static readonly HashSet<string> EscrituraCajero = new(StringComparer.Ordinal)
    {
        "Caja", "ArqueoDiario", "Egresos"
    };

    /// <summary>Acciones reservadas al administrador aunque el controlador sea de caja.</summary>
    private static readonly HashSet<string> SoloAdministrador = new(StringComparer.Ordinal)
    {
        "AnularRecibo", "Anular"
    };

    private readonly bool _exigirLogin;

    public PermisosPorRolFilter(IConfiguration configuracion)
    {
        _exigirLogin = configuracion.GetValue<bool>("MERCADEX_EXIGIR_LOGIN");
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor accion ||
            ConAutorizacionPropia.Contains(accion.ControllerName))
            return;

        var http = context.HttpContext;
        var usuario = http.User;

        if (usuario.Identity?.IsAuthenticated != true)
        {
            // Llegó una sesión pero no es válida (vencida, usuario desactivado,
            // contraseña cambiada): se pide volver a ingresar.
            var traeSesion = http.Request.Headers.Authorization.ToString()
                .StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

            if (traeSesion || _exigirLogin)
                context.Result = Respuesta(401, "Tu sesión terminó. Vuelve a ingresar.");

            return;
        }

        if (usuario.IsInRole(SeguridadApp.RolSocio))
        {
            context.Result = Respuesta(403, "Esta sesión no tiene acceso a Mercadex de escritorio.");
            return;
        }

        if (usuario.HasClaim(c => c.Type == SeguridadUsuarios.ClaimDebeCambiar))
        {
            context.Result = Respuesta(403, "Primero cambia tu contraseña temporal.");
            return;
        }

        var lectura = HttpMethods.IsGet(http.Request.Method) || HttpMethods.IsHead(http.Request.Method);

        if (usuario.IsInRole(SeguridadUsuarios.RolAdministrador))
            return;

        if (usuario.IsInRole(SeguridadUsuarios.RolCajero))
        {
            if (lectura ||
                (EscrituraCajero.Contains(accion.ControllerName) && !SoloAdministrador.Contains(accion.ActionName)))
                return;

            context.Result = Respuesta(403, "Tu usuario es de caja: esta acción la hace un administrador.");
            return;
        }

        if (usuario.IsInRole(SeguridadUsuarios.RolConsulta) && lectura)
            return;

        context.Result = Respuesta(403, "Tu usuario es de solo consulta: no puede modificar datos.");
    }

    private static ObjectResult Respuesta(int estado, string mensaje) =>
        new(new { mensaje }) { StatusCode = estado };
}
