using Mercadex.Domain.Common;

namespace Mercadex.Domain.Usuarios;

/// <summary>
/// Persona que usa Mercadex de escritorio o el panel de caja.
/// Solo se guarda la huella (hash) de la contraseña.
/// Cada acción que registra queda a su nombre (cobros, anulaciones,
/// modificaciones de deuda, cierres de caja).
/// </summary>
public sealed class Usuario : Entity
{
    public const int IntentosPermitidos = 5;
    public static readonly TimeSpan TiempoBloqueo = TimeSpan.FromMinutes(15);

    private Usuario()
    {
    }

    public Usuario(
        string nombreUsuario,
        string nombreCompleto,
        RolUsuario rol,
        string contrasenaHash,
        DateTimeOffset ahoraUtc)
    {
        NombreUsuario = NormalizarNombreUsuario(nombreUsuario);
        NombreCompleto = ValidarNombreCompleto(nombreCompleto);
        Rol = ValidarRol(rol);
        ContrasenaHash = ValidarHash(contrasenaHash);
        DebeCambiarContrasena = true;
        Activo = true;
        FechaCreacionUtc = ahoraUtc;
    }

    public string NombreUsuario { get; private set; } = "";

    public string NombreCompleto { get; private set; } = "";

    public RolUsuario Rol { get; private set; }

    public string ContrasenaHash { get; private set; } = "";

    /// <summary>
    /// Verdadero con una contraseña temporal (usuario nuevo o restablecido):
    /// al ingresar debe elegir la suya.
    /// </summary>
    public bool DebeCambiarContrasena { get; private set; }

    public bool Activo { get; private set; }

    public int IntentosFallidos { get; private set; }

    public DateTimeOffset? BloqueadoHastaUtc { get; private set; }

    /// <summary>Cambia cada vez que se invalidan las sesiones abiertas.</summary>
    public int VersionSesion { get; private set; }

    public DateTimeOffset FechaCreacionUtc { get; private set; }

    public DateTimeOffset? UltimoAccesoUtc { get; private set; }

    public bool EstaBloqueado(DateTimeOffset ahoraUtc) =>
        BloqueadoHastaUtc.HasValue && BloqueadoHastaUtc > ahoraUtc;

    public void Actualizar(string nombreCompleto, RolUsuario rol)
    {
        var nuevoRol = ValidarRol(rol);
        NombreCompleto = ValidarNombreCompleto(nombreCompleto);

        if (nuevoRol != Rol)
        {
            Rol = nuevoRol;
            VersionSesion++;
        }
    }

    /// <summary>El propio usuario elige su contraseña.</summary>
    public void CambiarContrasena(string contrasenaHash)
    {
        ContrasenaHash = ValidarHash(contrasenaHash);
        DebeCambiarContrasena = false;
        VersionSesion++;
    }

    /// <summary>El administrador entrega una contraseña temporal.</summary>
    public void RestablecerContrasena(string contrasenaTemporalHash)
    {
        ContrasenaHash = ValidarHash(contrasenaTemporalHash);
        DebeCambiarContrasena = true;
        IntentosFallidos = 0;
        BloqueadoHastaUtc = null;
        VersionSesion++;
    }

    public void Desactivar()
    {
        Activo = false;
        VersionSesion++;
    }

    public void Activar()
    {
        Activo = true;
        IntentosFallidos = 0;
        BloqueadoHastaUtc = null;
    }

    public void RegistrarFallo(DateTimeOffset ahoraUtc)
    {
        IntentosFallidos++;

        if (IntentosFallidos >= IntentosPermitidos)
        {
            BloqueadoHastaUtc = ahoraUtc + TiempoBloqueo;
            IntentosFallidos = 0;
        }
    }

    public void RegistrarAcceso(DateTimeOffset ahoraUtc)
    {
        IntentosFallidos = 0;
        BloqueadoHastaUtc = null;
        UltimoAccesoUtc = ahoraUtc;
    }

    public static string NormalizarNombreUsuario(string? nombreUsuario)
    {
        var limpio = (nombreUsuario ?? "").Trim().ToLowerInvariant();

        if (limpio.Length < 3 || limpio.Length > 40)
            throw new ArgumentException("El usuario debe tener entre 3 y 40 caracteres.", nameof(nombreUsuario));

        if (!limpio.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-'))
            throw new ArgumentException(
                "El usuario solo puede tener letras sin tilde, números, punto, guion o guion bajo.",
                nameof(nombreUsuario));

        return limpio;
    }

    private static string ValidarNombreCompleto(string? nombreCompleto)
    {
        var limpio = (nombreCompleto ?? "").Trim();

        if (limpio.Length < 3 || limpio.Length > 120)
            throw new ArgumentException("El nombre completo debe tener entre 3 y 120 caracteres.", nameof(nombreCompleto));

        return limpio;
    }

    private static RolUsuario ValidarRol(RolUsuario rol)
    {
        if (!Enum.IsDefined(rol))
            throw new ArgumentException("El rol no es válido.", nameof(rol));

        return rol;
    }

    private static string ValidarHash(string? hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("La contraseña es obligatoria.", nameof(hash));

        return hash;
    }
}
