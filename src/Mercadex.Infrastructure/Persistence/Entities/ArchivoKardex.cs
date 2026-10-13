namespace Mercadex.Infrastructure.Persistence.Entities;

/// <summary>
/// Contenido de un documento escaneado del Kardex guardado en la base.
/// Se usa cuando el servidor no tiene un disco persistente
/// (MERCADEX_KARDEX_STORAGE_PATH). El Id es el del DocumentoKardex.
/// </summary>
public sealed class ArchivoKardex
{
    public Guid Id { get; set; }

    public byte[] Contenido { get; set; } = Array.Empty<byte>();
}
