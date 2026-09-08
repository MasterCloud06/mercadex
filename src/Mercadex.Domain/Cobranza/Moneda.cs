using Mercadex.Domain.Common;

namespace Mercadex.Domain.Cobranza;

public sealed class Moneda : Entity
{
    private Moneda()
    {
    }

    public Moneda(
        string codigo,
        string nombre,
        string simbolo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código de la moneda es obligatorio.",
                nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre de la moneda es obligatorio.",
                nameof(nombre));
        }

        if (string.IsNullOrWhiteSpace(simbolo))
        {
            throw new ArgumentException(
                "El símbolo de la moneda es obligatorio.",
                nameof(simbolo));
        }

        Codigo = codigo.Trim().ToUpperInvariant();
        Nombre = nombre.Trim();
        Simbolo = simbolo.Trim();

        Activa = true;
    }

    public string Codigo { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;

    public string Simbolo { get; private set; } = string.Empty;

    public bool Activa { get; private set; }

    public void Activar()
    {
        Activa = true;
    }

    public void Desactivar()
    {
        Activa = false;
    }
}
