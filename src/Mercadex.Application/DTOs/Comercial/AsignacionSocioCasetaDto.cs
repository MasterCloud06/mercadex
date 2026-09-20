namespace Mercadex.Application.DTOs.Comercial;

public sealed record AsignacionSocioCasetaDto(
    Guid Id,
    Guid SocioId,
    Guid UnidadComercialId,
    DateTimeOffset FechaAsignacion,
    bool Activa);