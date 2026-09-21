namespace Mercadex.Application.DTOs.Caja;

public sealed record ItemCobroDto(
    Guid ObligacionId,
    string NombreConcepto
);