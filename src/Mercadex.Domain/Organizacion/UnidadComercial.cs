using Mercadex.Domain.Common;

namespace Mercadex.Domain.Organizacion;

public sealed class UnidadComercial : Entity
{
    private UnidadComercial()
    {
    }

    public UnidadComercial(
        string codigo,
        TipoUnidadComercial tipo,
        Guid? pasilloId = null,
        string? bloque = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código de la unidad comercial es obligatorio.",
                nameof(codigo));
        }

        Codigo = codigo.Trim().ToUpperInvariant();
        Tipo = tipo;
        PasilloId = pasilloId;
        Bloque = LimpiarOpcional(bloque);

        Activa = true;
        FechaRegistro = DateTimeOffset.UtcNow;
    }

    public string Codigo { get; private set; } = string.Empty;

    public TipoUnidadComercial Tipo { get; private set; }

    public Guid? PasilloId { get; private set; }

    public string? Bloque { get; private set; }

    public bool Activa { get; private set; }

    public DateTimeOffset FechaRegistro { get; private set; }

    public void CambiarUbicacion(
        Guid? pasilloId,
        string? bloque)
    {
        PasilloId = pasilloId;
        Bloque = LimpiarOpcional(bloque);
    }

    public void Activar()
    {
        Activa = true;
    }

    public void Desactivar()
    {
        Activa = false;
    }

    private static string? LimpiarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
    }
}
