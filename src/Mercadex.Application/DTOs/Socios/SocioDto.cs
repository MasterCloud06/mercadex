namespace Mercadex.Application.DTOs.Socios;

public record SocioDto(
    Guid Id,
    string NombreCompleto,
    string DocumentoIdentidad,
    string? Telefono,
    DateTimeOffset FechaRegistro,
    bool Activo);