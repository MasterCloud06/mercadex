namespace Mercadex.Application.DTOs.Cobranza;

public sealed record ResultadoGeneracionCobranzaDto(
    int CantidadGenerada,
    decimal TotalGenerado);