namespace Mercadex.Application.DTOs;

public sealed record CrearSocioDto(
    string Nombres,
    string Apellidos,
    string? DocumentoIdentidad,
    string? Telefono
);