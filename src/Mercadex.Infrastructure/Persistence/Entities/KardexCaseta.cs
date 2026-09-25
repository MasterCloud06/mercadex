namespace Mercadex.Infrastructure.Persistence.Entities;

public sealed class KardexCaseta
{
    public Guid Id { get; set; }

    public Guid UnidadComercialId { get; set; }

    public Guid? SocioId { get; set; }

    public DateOnly? FechaIngreso { get; set; }

    public DateTimeOffset FechaRegistro { get; set; }

    public string? EstadoVida { get; set; }

    public string? TipoDocumentoPropiedad { get; set; }

    public string? PropietarioAnterior { get; set; }

    public bool? AporteSocioPagado { get; set; }

    public string? Observaciones { get; set; }
}
