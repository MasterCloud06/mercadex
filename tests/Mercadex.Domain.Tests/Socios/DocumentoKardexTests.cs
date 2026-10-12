using Mercadex.Domain.Socios;

namespace Mercadex.Domain.Tests.Socios;

public sealed class DocumentoKardexTests
{
    // SHA-256 calculado sobre datos ficticios.
    // Siempre produce 64 caracteres hexadecimales.
    private static readonly string HashValido =
        Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                new byte[2048]));

    private static DocumentoKardex Crear(
        TipoDocumentoKardex tipo = TipoDocumentoKardex.Identidad)
    {
        return new DocumentoKardex(
            Guid.CreateVersion7(),
            null,
            tipo,
            "identidad.pdf",
            "application/pdf",
            2048,
            HashValido,
            "admin_tarifas_dev");
    }

    [Fact]
    public void CrearDocumento_Valido_DebeRegistrarMetadata()
    {
        var documento = Crear();

        Assert.Equal(TipoDocumentoKardex.Identidad, documento.Tipo);
        Assert.Equal("identidad.pdf", documento.NombreArchivo);
        Assert.Equal("application/pdf", documento.TipoContenido);
        Assert.Equal(2048, documento.TamanoBytes);
        Assert.Equal(64, documento.Sha256.Length);
        Assert.Equal(HashValido, documento.Sha256);
        Assert.Equal("admin_tarifas_dev", documento.RegistradoPor);
        Assert.Equal(32, documento.ClaveArchivo.Length);
        Assert.True(documento.FechaRegistroUtc <= DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData(TipoDocumentoKardex.Identidad)]
    [InlineData(TipoDocumentoKardex.Defuncion)]
    [InlineData(TipoDocumentoKardex.Titularidad)]
    [InlineData(TipoDocumentoKardex.ComprobanteAporte)]
    [InlineData(TipoDocumentoKardex.Fotografia)]
    public void TiposPermitidos_DebenRegistrarse(
        TipoDocumentoKardex tipo)
    {
        Assert.Equal(tipo, Crear(tipo).Tipo);
    }

    [Fact]
    public void SocioVacio_DebeRechazar()
    {
        Assert.Throws<ArgumentException>(() =>
            new DocumentoKardex(
                Guid.Empty, null,
                TipoDocumentoKardex.Identidad,
                "identidad.pdf", "application/pdf",
                100, HashValido, "admin_tarifas_dev"));
    }

    [Fact]
    public void MayorDe10MB_DebeRechazar()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DocumentoKardex(
                Guid.CreateVersion7(), null,
                TipoDocumentoKardex.Identidad,
                "identidad.pdf", "application/pdf",
                DocumentoKardex.TamanoMaximo + 1,
                HashValido, "admin_tarifas_dev"));
    }

    [Fact]
    public void RutaInsegura_DebeRechazar()
    {
        Assert.Throws<ArgumentException>(() =>
            new DocumentoKardex(
                Guid.CreateVersion7(), null,
                TipoDocumentoKardex.Identidad,
                "../identidad.pdf", "application/pdf",
                100, HashValido, "admin_tarifas_dev"));
    }

    [Fact]
    public void TipoContenidoInvalido_DebeRechazar()
    {
        Assert.Throws<ArgumentException>(() =>
            new DocumentoKardex(
                Guid.CreateVersion7(), null,
                TipoDocumentoKardex.Identidad,
                "archivo.exe", "application/octet-stream",
                100, HashValido, "admin_tarifas_dev"));
    }

    [Fact]
    public void HashInvalido_DebeRechazar()
    {
        Assert.Throws<ArgumentException>(() =>
            new DocumentoKardex(
                Guid.CreateVersion7(), null,
                TipoDocumentoKardex.Identidad,
                "identidad.pdf", "application/pdf",
                100, "123", "admin_tarifas_dev"));
    }

    [Fact]
    public void PuedeVincularseACaseta()
    {
        var casetaId = Guid.CreateVersion7();

        var documento = new DocumentoKardex(
            Guid.CreateVersion7(), casetaId,
            TipoDocumentoKardex.Titularidad,
            "titularidad.pdf", "application/pdf",
            4096, HashValido, "admin_tarifas_dev");

        Assert.Equal(casetaId, documento.UnidadComercialId);
    }
}