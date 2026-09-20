namespace Mercadex.Application.DTOs.Comercial;

public sealed record CrearAsignacionSocioCasetaDto(
    Guid SocioId,
    Guid UnidadComercialId);