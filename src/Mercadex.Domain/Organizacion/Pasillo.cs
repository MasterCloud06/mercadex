using Mercadex.Domain.Common;

namespace Mercadex.Domain.Organizacion;

public sealed class Pasillo : Entity
{
    private Pasillo()
    {
    }

    public Pasillo(
        string codigo,
        string nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código del pasillo es obligatorio.",
                nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre del pasillo es obligatorio.",
                nameof(nombre));
        }

        Codigo = codigo.Trim().ToUpperInvariant();
        Nombre = nombre.Trim();
        Activo = true;
    }

    public string Codigo { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;

    public bool Activo { get; private set; }

    public void Activar()
    {
        Activo = true;
    }

    public void Desactivar()
    {
        Activo = false;
    }
}
