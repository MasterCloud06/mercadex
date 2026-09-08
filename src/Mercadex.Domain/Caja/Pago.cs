using Mercadex.Domain.Common;

namespace Mercadex.Domain.Caja;

public sealed class Pago : Entity
{
    private Pago()
    {
    }

    public Pago(
        Guid monedaId,
        decimal monto,
        FormaPago formaPago,
        string nombrePagador,
        DateTimeOffset fechaRegistro,
        decimal tipoCambio = 1m,
        string? referencia = null)
    {
        if (monedaId == Guid.Empty)
        {
            throw new ArgumentException(
                "La moneda es obligatoria.",
                nameof(monedaId));
        }

        if (monto <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(monto),
                "El monto del pago debe ser mayor a cero.");
        }

        if (string.IsNullOrWhiteSpace(nombrePagador))
        {
            throw new ArgumentException(
                "El nombre del pagador es obligatorio.",
                nameof(nombrePagador));
        }

        if (tipoCambio <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tipoCambio),
                "El tipo de cambio debe ser mayor a cero.");
        }

        MonedaId = monedaId;
        Monto = monto;
        FormaPago = formaPago;

        NombrePagador = nombrePagador.Trim();
        Referencia = LimpiarOpcional(referencia);

        TipoCambio = tipoCambio;
        FechaRegistro = fechaRegistro;

        Estado = EstadoPago.Registrado;
    }

    public Guid MonedaId { get; private set; }

    public decimal Monto { get; private set; }

    public FormaPago FormaPago { get; private set; }

    public string NombrePagador { get; private set; } = string.Empty;

    public string? Referencia { get; private set; }

    public decimal TipoCambio { get; private set; }

    public DateTimeOffset FechaRegistro { get; private set; }

    public EstadoPago Estado { get; private set; }

    public bool EstaAnulado =>
        Estado == EstadoPago.Anulado;

    public void Anular()
    {
        if (Estado == EstadoPago.Anulado)
        {
            throw new InvalidOperationException(
                "El pago ya se encuentra anulado.");
        }

        Estado = EstadoPago.Anulado;
    }

    private static string? LimpiarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
    }
}
