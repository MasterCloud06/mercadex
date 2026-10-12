using Mercadex.Domain.Common;

namespace Mercadex.Domain.AppSocios;

/// <summary>
/// Acceso de un socio a la aplicación móvil.
/// La caja genera un código de activación (de un solo uso y con
/// vencimiento); con él el socio crea su contraseña. Generar un
/// código nuevo anula la contraseña y cierra las sesiones abiertas.
/// Solo se guardan huellas (hash), nunca el código ni la contraseña.
/// </summary>
public sealed class CuentaSocioApp : Entity
{
    public const int IntentosPermitidos = 5;
    public static readonly TimeSpan TiempoBloqueo = TimeSpan.FromMinutes(15);

    private CuentaSocioApp()
    {
    }

    public CuentaSocioApp(Guid socioId, DateTimeOffset ahoraUtc)
    {
        if (socioId == Guid.Empty)
            throw new ArgumentException("El socio es obligatorio.", nameof(socioId));

        SocioId = socioId;
        FechaCreacionUtc = ahoraUtc;
        Activa = true;
    }

    public Guid SocioId { get; private set; }

    public string? ContrasenaHash { get; private set; }

    public string? CodigoActivacionHash { get; private set; }

    public DateTimeOffset? CodigoExpiraUtc { get; private set; }

    public int IntentosFallidos { get; private set; }

    public DateTimeOffset? BloqueadaHastaUtc { get; private set; }

    public bool Activa { get; private set; }

    /// <summary>Cambia cada vez que se invalidan las sesiones abiertas.</summary>
    public int VersionSesion { get; private set; }

    public DateTimeOffset FechaCreacionUtc { get; private set; }

    public DateTimeOffset? FechaActivacionUtc { get; private set; }

    public DateTimeOffset? UltimoAccesoUtc { get; private set; }

    public bool TieneContrasena => ContrasenaHash != null;

    public bool CodigoVigente(DateTimeOffset ahoraUtc) =>
        CodigoActivacionHash != null && CodigoExpiraUtc > ahoraUtc;

    public bool EstaBloqueada(DateTimeOffset ahoraUtc) =>
        BloqueadaHastaUtc.HasValue && BloqueadaHastaUtc > ahoraUtc;

    /// <summary>Nuevo código: anula la contraseña y las sesiones.</summary>
    public void AsignarCodigo(string codigoHash, DateTimeOffset expiraUtc)
    {
        CodigoActivacionHash = codigoHash ?? throw new ArgumentNullException(nameof(codigoHash));
        CodigoExpiraUtc = expiraUtc;
        ContrasenaHash = null;
        IntentosFallidos = 0;
        BloqueadaHastaUtc = null;
        Activa = true;
        VersionSesion++;
    }

    public void Activar(string contrasenaHash, DateTimeOffset ahoraUtc)
    {
        ContrasenaHash = contrasenaHash ?? throw new ArgumentNullException(nameof(contrasenaHash));
        CodigoActivacionHash = null;
        CodigoExpiraUtc = null;
        FechaActivacionUtc = ahoraUtc;
        RegistrarAcceso(ahoraUtc);
    }

    public void CambiarContrasena(string contrasenaHash)
    {
        ContrasenaHash = contrasenaHash ?? throw new ArgumentNullException(nameof(contrasenaHash));
        VersionSesion++;
    }

    public void RegistrarFallo(DateTimeOffset ahoraUtc)
    {
        IntentosFallidos++;
        if (IntentosFallidos >= IntentosPermitidos)
        {
            BloqueadaHastaUtc = ahoraUtc + TiempoBloqueo;
            IntentosFallidos = 0;
        }
    }

    public void RegistrarAcceso(DateTimeOffset ahoraUtc)
    {
        IntentosFallidos = 0;
        BloqueadaHastaUtc = null;
        UltimoAccesoUtc = ahoraUtc;
    }

    public void Desactivar()
    {
        Activa = false;
        ContrasenaHash = null;
        CodigoActivacionHash = null;
        CodigoExpiraUtc = null;
        VersionSesion++;
    }
}
