namespace Mercadex.Application.DTOs;

public sealed record ActualizarSocioDto(
    string Nombres,
    string Apellidos,
    string? DocumentoIdentidad,
    string? Telefono
);