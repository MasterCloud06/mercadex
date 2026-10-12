namespace Mercadex.Domain.Finanzas;

public sealed class Egreso
{
    private Egreso()
    {
    }

    public Egreso(
        DateOnly fecha,
        string categoria,
        string concepto,
        string beneficiario,
        decimal monto,
        Guid monedaId,
        string formaPago,
        string? numeroComprobante,
        string? observacion)
    {
        if (string.IsNullOrWhiteSpace(categoria))
            throw new ArgumentException(
                "La categoría es obligatoria.",
                nameof(categoria));

        if (string.IsNullOrWhiteSpace(concepto))
            throw new ArgumentException(
                "El concepto es obligatorio.",
                nameof(concepto));

        if (string.IsNullOrWhiteSpace(beneficiario))
            throw new ArgumentException(
                "El beneficiario o proveedor es obligatorio.",
                nameof(beneficiario));

        if (monto <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(monto),
                "El monto debe ser mayor a cero.");

        if (monedaId == Guid.Empty)
            throw new ArgumentException(
                "La moneda es obligatoria.",
                nameof(monedaId));

        if (string.IsNullOrWhiteSpace(formaPago))
            throw new ArgumentException(
                "La forma de pago es obligatoria.",
                nameof(formaPago));

        categoria = categoria.Trim();
        concepto = concepto.Trim();
        beneficiario = beneficiario.Trim();
        formaPago = formaPago.Trim();

        if (categoria.Length > 100)
            throw new ArgumentException(
                "La categoría no puede superar los 100 caracteres.");

        if (concepto.Length > 200)
            throw new ArgumentException(
                "El concepto no puede superar los 200 caracteres.");

        if (beneficiario.Length > 200)
            throw new ArgumentException(
                "El beneficiario no puede superar los 200 caracteres.");

        // "Otro" (versiones anteriores) se normaliza a "Otros".
        if (string.Equals(formaPago, "Otro", StringComparison.OrdinalIgnoreCase))
            formaPago = "Otros";

        var formaValida = FormasPagoValidas.FirstOrDefault(f =>
            string.Equals(f, formaPago, StringComparison.OrdinalIgnoreCase));

        if (formaValida is null)
            throw new ArgumentException(
                "Forma de pago no válida. Use: " + string.Join(", ", FormasPagoValidas) + ".");

        formaPago = formaValida;

        if (formaPago.Length > 50)
            throw new ArgumentException(
                "La forma de pago no puede superar los 50 caracteres.");

        if (numeroComprobante?.Trim().Length > 100)
            throw new ArgumentException(
                "El número de comprobante no puede superar los 100 caracteres.");

        if (observacion?.Trim().Length > 1000)
            throw new ArgumentException(
                "La observación no puede superar los 1000 caracteres.");

        Id = Guid.NewGuid();

        Fecha = fecha;
        Categoria = categoria;
        Concepto = concepto;
        Beneficiario = beneficiario;
        Monto = monto;
        MonedaId = monedaId;
        FormaPago = formaPago;

        NumeroComprobante =
            string.IsNullOrWhiteSpace(numeroComprobante)
                ? null
                : numeroComprobante.Trim();

        Observacion =
            string.IsNullOrWhiteSpace(observacion)
                ? null
                : observacion.Trim();

        FechaRegistro = DateTimeOffset.UtcNow;
        Anulado = false;
    }

    // Formas de pago aceptadas para egresos.
    public static readonly IReadOnlyList<string> FormasPagoValidas =
        new[] { "Efectivo", "QR", "Transferencia", "Cheque", "Otros" };

    public Guid Id { get; private set; }

    /// <summary>Número correlativo del comprobante de egreso (lo asigna la base).</summary>
    public long Numero { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string Categoria { get; private set; } = string.Empty;

    public string Concepto { get; private set; } = string.Empty;

    public string Beneficiario { get; private set; } = string.Empty;

    public decimal Monto { get; private set; }

    public Guid MonedaId { get; private set; }

    public string FormaPago { get; private set; } = string.Empty;

    public string? NumeroComprobante { get; private set; }

    public string? Observacion { get; private set; }

    public DateTimeOffset FechaRegistro { get; private set; }

    public bool Anulado { get; private set; }

    public DateTimeOffset? FechaAnulacion { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public void Anular(
        string motivo,
        DateTimeOffset fecha)
    {
        if (Anulado)
            throw new InvalidOperationException(
                "El egreso ya está anulado.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException(
                "El motivo de anulación es obligatorio.",
                nameof(motivo));

        motivo = motivo.Trim();

        if (motivo.Length > 500)
            throw new ArgumentException(
                "El motivo de anulación no puede superar los 500 caracteres.");

        Anulado = true;
        FechaAnulacion = fecha;
        MotivoAnulacion = motivo;
    }
}