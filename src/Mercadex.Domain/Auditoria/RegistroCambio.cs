using Mercadex.Domain.Common;

namespace Mercadex.Domain.Auditoria;

/// <summary>
/// Una línea del registro de cambios: quién hizo qué, cuándo y desde dónde.
/// Solo se agregan líneas; el sistema no ofrece forma de editarlas ni borrarlas.
/// </summary>
public sealed class RegistroCambio : Entity
{
    private RegistroCambio()
    {
    }

    public RegistroCambio(
        DateTimeOffset fechaUtc,
        string? usuarioId,
        string? usuario,
        string? rol,
        string origen,
        string modulo,
        string accion,
        string? detalle,
        string? referencia,
        string? ip)
    {
        FechaUtc = fechaUtc;
        UsuarioId = Recortar(usuarioId, 64);
        Usuario = Recortar(usuario, 120) ?? "Sin sesión";
        Rol = Recortar(rol, 30);
        Origen = Recortar(origen, 30) ?? "Escritorio";
        Modulo = Recortar(modulo, 60) ?? "General";
        Accion = Recortar(accion, 120) ?? "Cambio";
        Detalle = Recortar(detalle, 1000);
        Referencia = Recortar(referencia, 64);
        Ip = Recortar(ip, 45);
    }

    public DateTimeOffset FechaUtc { get; private set; }

    public string? UsuarioId { get; private set; }

    public string Usuario { get; private set; } = "";

    public string? Rol { get; private set; }

    /// <summary>Escritorio, Panel web o App de socios.</summary>
    public string Origen { get; private set; } = "";

    public string Modulo { get; private set; } = "";

    public string Accion { get; private set; } = "";

    public string? Detalle { get; private set; }

    /// <summary>Identificador del registro afectado (recibo, socio, usuario…).</summary>
    public string? Referencia { get; private set; }

    public string? Ip { get; private set; }

    private static string? Recortar(string? texto, int maximo)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return null;

        var limpio = texto.Trim().ReplaceLineEndings(" ");
        return limpio.Length > maximo ? limpio[..maximo] : limpio;
    }
}
