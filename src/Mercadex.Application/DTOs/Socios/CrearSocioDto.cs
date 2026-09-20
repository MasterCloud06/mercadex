namespace Mercadex.Application.DTOs.Socios;

public record CrearSocioDto(
    string Nombres,
    string Apellidos,
    string DocumentoIdentidad,
    string? Telefono);