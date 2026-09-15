namespace Mercadex.Application.DTOs;

public sealed record SocioDto(
    Guid Id,
    string Nombres,
    string Apellidos,
    string? DocumentoIdentidad,
    string? Telefono,
    bool Activo
);