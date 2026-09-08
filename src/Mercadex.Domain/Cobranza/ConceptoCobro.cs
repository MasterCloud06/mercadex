using Mercadex.Domain.Common;

namespace Mercadex.Domain.Cobranza;

public sealed class ConceptoCobro : Entity
{
    private ConceptoCobro()
    {
    }

    public ConceptoCobro(
        string codigo,
        string nombre,
        string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código del concepto es obligatorio.",
                nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre del concepto es obligatorio.",
                nameof(nombre));
        }

        Codigo = codigo.Trim().ToUpperInvariant();
        Nombre = nombre.Trim();
        Descripcion = LimpiarOpcional(descripcion);

        Activo = true;
    }

    public string Codigo { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;

    public string? Descripcion { get; private set; }

    public bool Activo { get; private set; }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre del concepto es obligatorio.",
                nameof(nombre));
        }

        Nombre = nombre.Trim();
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
