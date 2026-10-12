using Mercadex.Domain.Common;

namespace Mercadex.Domain.Socios;

public sealed class DocumentoKardex : Entity
{
    public const long TamanoMaximo = 10 * 1024 * 1024;

    private DocumentoKardex() { }

    public DocumentoKardex(
        Guid socioId,
        Guid? unidadComercialId,
        TipoDocumentoKardex tipo,
        string nombreArchivo,
        string tipoContenido,
        long tamanoBytes,
        string sha256,
        string registradoPor)
    {
        if (socioId == Guid.Empty)
            throw new ArgumentException(
                "El socio es obligatorio.", nameof(socioId));

        if (unidadComercialId == Guid.Empty)
            throw new ArgumentException(
                "La caseta indicada no es valida.",
                nameof(unidadComercialId));

        if (!Enum.IsDefined(tipo))
            throw new ArgumentException(
                "Tipo de documento invalido.", nameof(tipo));

        if (string.IsNullOrWhiteSpace(nombreArchivo) ||
            nombreArchivo.Length > 255 ||
            nombreArchivo.Contains('/') ||
            nombreArchivo.Contains('\\') ||
            nombreArchivo.Contains(".."))
        {
            throw new ArgumentException(
                "Nombre de archivo invalido.",
                nameof(nombreArchivo));
        }

        var contenido = tipoContenido?.Trim().ToLowerInvariant();

        if (contenido is not (
            "application/pdf" or
            "image/jpeg" or
            "image/png"))
        {
            throw new ArgumentException(
                "Solo PDF, JPG y PNG.",
                nameof(tipoContenido));
        }

        var extension =
            Path.GetExtension(nombreArchivo).ToLowerInvariant();

        bool extensionValida = contenido switch
        {
            "application/pdf" => extension == ".pdf",
            "image/jpeg" => extension is ".jpg" or ".jpeg",
            "image/png" => extension == ".png",
            _ => false
        };

        if (!extensionValida)
            throw new ArgumentException(
                "La extension no coincide con el contenido.",
                nameof(nombreArchivo));

        if (tamanoBytes <= 0 || tamanoBytes > TamanoMaximo)
            throw new ArgumentOutOfRangeException(
                nameof(tamanoBytes),
                "El archivo debe tener entre 1 byte y 10 MB.");

        if (string.IsNullOrWhiteSpace(sha256) ||
            sha256.Length != 64 ||
            !sha256.All(Uri.IsHexDigit))
        {
            throw new ArgumentException(
                "SHA-256 invalido: se requieren 64 caracteres hexadecimales.",
                nameof(sha256));
        }

        if (string.IsNullOrWhiteSpace(registradoPor) ||
            registradoPor.Trim().Length > 120)
        {
            throw new ArgumentException(
                "Responsable obligatorio.",
                nameof(registradoPor));
        }

        SocioId = socioId;
        UnidadComercialId = unidadComercialId;
        Tipo = tipo;
        NombreArchivo = nombreArchivo.Trim();
        TipoContenido = contenido!;
        TamanoBytes = tamanoBytes;
        Sha256 = sha256.ToUpperInvariant();
        RegistradoPor = registradoPor.Trim();

        ClaveArchivo = Guid.CreateVersion7().ToString("N");
        FechaRegistroUtc = DateTimeOffset.UtcNow;
    }

    public Guid SocioId { get; private set; }
    public Guid? UnidadComercialId { get; private set; }
    public TipoDocumentoKardex Tipo { get; private set; }
    public string NombreArchivo { get; private set; } = "";
    public string TipoContenido { get; private set; } = "";
    public long TamanoBytes { get; private set; }
    public string Sha256 { get; private set; } = "";
    public string ClaveArchivo { get; private set; } = "";
    public string RegistradoPor { get; private set; } = "";
    public DateTimeOffset FechaRegistroUtc { get; private set; }
}