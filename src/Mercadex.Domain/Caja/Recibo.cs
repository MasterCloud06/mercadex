using Mercadex.Domain.Common;

namespace Mercadex.Domain.Caja;

public sealed class Recibo : Entity
{
    private readonly List<DetalleRecibo> _detalles = [];

    private Recibo()
    {
    }

    public Recibo(
        long numero,
        Guid unidadComercialId,
        Guid pagoId,
        DateTimeOffset fechaEmision,
        IEnumerable<DetalleRecibo> detalles)
    {
        if (numero <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numero),
                "El número de recibo debe ser mayor a cero.");
        }

        if (unidadComercialId == Guid.Empty)
        {
            throw new ArgumentException(
                "La unidad comercial es obligatoria.",
                nameof(unidadComercialId));
        }

        if (pagoId == Guid.Empty)
        {
            throw new ArgumentException(
                "El pago es obligatorio.",
                nameof(pagoId));
        }

        ArgumentNullException.ThrowIfNull(detalles);

        var lista = detalles.ToList();

        if (lista.Count == 0)
        {
            throw new ArgumentException(
                "El recibo debe contener al menos un detalle.",
                nameof(detalles));
        }

        if (lista
            .GroupBy(x => x.ObligacionId)
            .Any(x => x.Count() > 1))
        {
            throw new InvalidOperationException(
                "Una obligación no puede aparecer más de una vez en el recibo.");
        }

        Numero = numero;
        UnidadComercialId = unidadComercialId;
        PagoId = pagoId;
        FechaEmision = fechaEmision;

        Estado = EstadoRecibo.Emitido;

        _detalles.AddRange(lista);
    }

    public long Numero { get; private set; }

    public Guid UnidadComercialId { get; private set; }

    public Guid PagoId { get; private set; }

    public DateTimeOffset FechaEmision { get; private set; }

    public EstadoRecibo Estado { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public DateTimeOffset? FechaAnulacion { get; private set; }

    public string? AnuladoPor { get; private set; }

    /// <summary>Usuario que registró el cobro (vacío en recibos anteriores al inicio de sesión).</summary>
    public string? RegistradoPor { get; private set; }

    public void AsignarRegistradoPor(string? usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario))
            return;

        var limpio = usuario.Trim();
        RegistradoPor = limpio.Length > 120 ? limpio[..120] : limpio;
    }

    public IReadOnlyCollection<DetalleRecibo> Detalles =>
        _detalles.AsReadOnly();

    public decimal Total =>
        _detalles.Sum(x => x.Monto);

    public bool EstaAnulado =>
        Estado == EstadoRecibo.Anulado;

    public void Anular(
        string motivo,
        DateTimeOffset fechaAnulacion)
    {
        Anular(
            motivo,
            fechaAnulacion,
            "Sistema");
    }

    public void Anular(
        string motivo,
        DateTimeOffset fechaAnulacion,
        string anuladoPor)
    {
        if (Estado == EstadoRecibo.Anulado)
        {
            throw new InvalidOperationException(
                "El recibo ya se encuentra anulado.");
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ArgumentException(
                "El motivo de anulación es obligatorio.",
                nameof(motivo));
        }

        motivo = motivo.Trim();

        if (motivo.Length > 500)
        {
            throw new ArgumentException(
                "El motivo de anulación no puede superar 500 caracteres.",
                nameof(motivo));
        }

        if (fechaAnulacion < FechaEmision)
        {
            throw new ArgumentException(
                "La fecha de anulación no puede ser anterior a la emisión.",
                nameof(fechaAnulacion));
        }

        if (string.IsNullOrWhiteSpace(anuladoPor))
        {
            throw new ArgumentException(
                "Debe indicarse quién realizó la anulación.",
                nameof(anuladoPor));
        }

        anuladoPor = anuladoPor.Trim();

        if (anuladoPor.Length > 150)
        {
            throw new ArgumentException(
                "El usuario que anula no puede superar 150 caracteres.",
                nameof(anuladoPor));
        }

        Estado = EstadoRecibo.Anulado;
        MotivoAnulacion = motivo;
        FechaAnulacion = fechaAnulacion;
        AnuladoPor = anuladoPor;
    }
}