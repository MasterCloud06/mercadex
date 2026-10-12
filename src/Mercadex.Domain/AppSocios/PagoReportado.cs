using Mercadex.Domain.Common;

namespace Mercadex.Domain.AppSocios;

public enum EstadoPagoReportado
{
    Pendiente = 1,
    Aprobado = 2,
    Rechazado = 3
}

/// <summary>
/// Pago que el socio informa desde la app: pagó por QR o transferencia
/// a la cuenta de la asociación y adjunta la foto del comprobante.
/// La caja lo revisa: al aprobarlo se registra el cobro y se emite el
/// recibo; al rechazarlo se indica el motivo. Las deudas no cambian
/// hasta la aprobación.
/// </summary>
public sealed class PagoReportado : Entity
{
    public const int TamanoMaximoComprobante = 4 * 1024 * 1024;

    private PagoReportado()
    {
    }

    public PagoReportado(
        Guid socioId,
        Guid unidadComercialId,
        Guid monedaId,
        decimal monto,
        IEnumerable<Guid> obligacionIds,
        string referencia,
        byte[] comprobante,
        string comprobanteTipo,
        DateTimeOffset ahoraUtc)
    {
        var ids = obligacionIds?.Distinct().ToList() ?? new List<Guid>();

        if (socioId == Guid.Empty || unidadComercialId == Guid.Empty || monedaId == Guid.Empty)
            throw new ArgumentException("Datos del pago incompletos.");

        if (ids.Count == 0)
            throw new ArgumentException("Elige al menos una deuda.");

        if (monto <= 0)
            throw new ArgumentException("El monto debe ser mayor que cero.");

        referencia = referencia?.Trim() ?? "";
        if (referencia.Length is < 3 or > 80)
            throw new ArgumentException("Escribe el número de transacción que figura en el comprobante.");

        if (comprobante is null || comprobante.Length == 0)
            throw new ArgumentException("Adjunta la foto del comprobante.");

        if (comprobante.Length > TamanoMaximoComprobante)
            throw new ArgumentException("La foto del comprobante es demasiado grande (máximo 4 MB).");

        if (comprobanteTipo is not ("image/jpeg" or "image/png"))
            throw new ArgumentException("El comprobante debe ser una foto (JPG o PNG).");

        SocioId = socioId;
        UnidadComercialId = unidadComercialId;
        MonedaId = monedaId;
        Monto = monto;
        ObligacionIds = ids;
        Referencia = referencia;
        Comprobante = comprobante;
        ComprobanteTipo = comprobanteTipo;
        FechaReporteUtc = ahoraUtc;
        Estado = EstadoPagoReportado.Pendiente;
    }

    public Guid SocioId { get; private set; }
    public Guid UnidadComercialId { get; private set; }
    public Guid MonedaId { get; private set; }
    public decimal Monto { get; private set; }
    public List<Guid> ObligacionIds { get; private set; } = new();
    public string Referencia { get; private set; } = "";
    public byte[] Comprobante { get; private set; } = Array.Empty<byte>();
    public string ComprobanteTipo { get; private set; } = "image/jpeg";
    public DateTimeOffset FechaReporteUtc { get; private set; }
    public EstadoPagoReportado Estado { get; private set; }
    public string? MotivoRechazo { get; private set; }
    public string? RevisadoPor { get; private set; }
    public DateTimeOffset? FechaRevisionUtc { get; private set; }
    public Guid? ReciboId { get; private set; }
    public long? NumeroRecibo { get; private set; }

    public void Aprobar(string revisadoPor, Guid reciboId, long numeroRecibo, DateTimeOffset ahoraUtc)
    {
        AsegurarPendiente();
        RevisadoPor = Responsable(revisadoPor);
        ReciboId = reciboId;
        NumeroRecibo = numeroRecibo;
        FechaRevisionUtc = ahoraUtc;
        Estado = EstadoPagoReportado.Aprobado;
    }

    public void Rechazar(string revisadoPor, string motivo, DateTimeOffset ahoraUtc)
    {
        AsegurarPendiente();

        motivo = motivo?.Trim() ?? "";
        if (motivo.Length is < 5 or > 300)
            throw new ArgumentException("Explica al socio por qué se rechaza (5 a 300 caracteres).");

        RevisadoPor = Responsable(revisadoPor);
        MotivoRechazo = motivo;
        FechaRevisionUtc = ahoraUtc;
        Estado = EstadoPagoReportado.Rechazado;
    }

    private void AsegurarPendiente()
    {
        if (Estado != EstadoPagoReportado.Pendiente)
            throw new InvalidOperationException("Este pago ya fue revisado.");
    }

    private static string Responsable(string? nombre)
    {
        nombre = nombre?.Trim() ?? "";
        return nombre.Length is >= 3 and <= 120
            ? nombre
            : throw new ArgumentException("Escribe el nombre de quien revisa (3 a 120 caracteres).");
    }
}
