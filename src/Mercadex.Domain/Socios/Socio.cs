using Mercadex.Domain.Common;

namespace Mercadex.Domain.Socios;

public sealed class Socio : Entity
{
    private Socio()
    {
    }

    public Socio(
        string nombres,
        string apellidos,
        string? documentoIdentidad = null,
        string? telefono = null)
    {
        CambiarNombre(nombres, apellidos);

        DocumentoIdentidad = LimpiarOpcional(documentoIdentidad);
        Telefono = LimpiarOpcional(telefono);

        Activo = true;
        FechaRegistro = DateTimeOffset.UtcNow;
    }

    public string Nombres { get; private set; } = string.Empty;

    public string Apellidos { get; private set; } = string.Empty;

    public string? DocumentoIdentidad { get; private set; }

    public string? Telefono { get; private set; }

    public bool Activo { get; private set; }

    public DateTimeOffset FechaRegistro { get; private set; }

    public string NombreCompleto =>
        $"{Nombres} {Apellidos}".Trim();

    public void CambiarNombre(
        string nombres,
        string apellidos)
    {
        if (string.IsNullOrWhiteSpace(nombres))
        {
            throw new ArgumentException(
                "Los nombres son obligatorios.",
                nameof(nombres));
        }

        if (string.IsNullOrWhiteSpace(apellidos))
        {
            throw new ArgumentException(
                "Los apellidos son obligatorios.",
                nameof(apellidos));
        }

        Nombres = nombres.Trim();
        Apellidos = apellidos.Trim();
    }

    public void CambiarTelefono(string? telefono)
    {
        Telefono = LimpiarOpcional(telefono);
    }

    public void Activar()
    {
        Activo = true;
    }

    public void Desactivar()
    {
        Activo = false;
    }

    private static string? LimpiarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
    }
}
