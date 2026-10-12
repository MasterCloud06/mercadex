using Mercadex.Domain.Common;

namespace Mercadex.Domain.AppSocios;

/// <summary>
/// Datos que la app muestra para pagar: QR de cobro de la asociación,
/// cuenta bancaria e instrucciones. Existe un solo registro.
/// </summary>
public sealed class ConfiguracionPagoApp : Entity
{
    private ConfiguracionPagoApp()
    {
    }

    public ConfiguracionPagoApp(DateTimeOffset ahoraUtc)
    {
        FechaActualizacionUtc = ahoraUtc;
    }

    public byte[]? QrImagen { get; private set; }
    public string? QrTipo { get; private set; }
    public string? Banco { get; private set; }
    public string? NumeroCuenta { get; private set; }
    public string? TitularCuenta { get; private set; }
    public string? Instrucciones { get; private set; }
    public DateTimeOffset FechaActualizacionUtc { get; private set; }

    public void Actualizar(
        string? banco,
        string? numeroCuenta,
        string? titularCuenta,
        string? instrucciones,
        DateTimeOffset ahoraUtc)
    {
        Banco = Limitar(banco, 80);
        NumeroCuenta = Limitar(numeroCuenta, 60);
        TitularCuenta = Limitar(titularCuenta, 120);
        Instrucciones = Limitar(instrucciones, 600);
        FechaActualizacionUtc = ahoraUtc;
    }

    public void CambiarQr(byte[]? imagen, string? tipo, DateTimeOffset ahoraUtc)
    {
        if (imagen is { Length: > 2 * 1024 * 1024 })
            throw new ArgumentException("La imagen del QR es demasiado grande (máximo 2 MB).");

        if (imagen != null && tipo is not ("image/png" or "image/jpeg"))
            throw new ArgumentException("El QR debe ser una imagen PNG o JPG.");

        QrImagen = imagen;
        QrTipo = imagen == null ? null : tipo;
        FechaActualizacionUtc = ahoraUtc;
    }

    private static string? Limitar(string? valor, int maximo)
    {
        valor = string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        if (valor?.Length > maximo)
            throw new ArgumentException($"Un dato supera los {maximo} caracteres.");
        return valor;
    }
}
