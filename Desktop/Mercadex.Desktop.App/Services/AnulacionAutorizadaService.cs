using Mercadex.Desktop.App.Api;
using Mercadex.Desktop.App.Configuration;

namespace Mercadex.Desktop.App.Services;

public sealed class AnulacionAutorizadaService
{
    private readonly ApiClient _api = new();

    public async Task AnularEnPruebasAsync(
        Guid reciboId,
        string motivo,
        string usuario,
        string contrasena)
    {
        // PROTECCION TEMPORAL:
        // No permitir operaciones de anulacion
        // contra Render ni otros servidores externos.
        var host = ApiConfiguration.BaseUri.Host;

        if (!string.Equals(
                host,
                "localhost",
                StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(
                host,
                "127.0.0.1",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "La anulacion definitiva esta bloqueada " +
                "hasta finalizar las pruebas con una base independiente.");
        }

        if (reciboId == Guid.Empty)
        {
            throw new ArgumentException(
                "Selecciona un recibo valido.");
        }

        if (string.IsNullOrWhiteSpace(motivo) ||
            motivo.Trim().Length < 10 ||
            motivo.Trim().Length > 500)
        {
            throw new ArgumentException(
                "El motivo debe tener entre 10 y 500 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(usuario) ||
            string.IsNullOrWhiteSpace(contrasena))
        {
            throw new ArgumentException(
                "Se requiere usuario y contrasena de administrador.");
        }

        var sesion = await _api.PostAsync<
            object,
            SesionAnulacionRespuesta>(
            "/api/auth/login",
            new
            {
                usuario = usuario.Trim(),
                password = contrasena
            });

        if (string.IsNullOrWhiteSpace(sesion?.Token))
        {
            throw new InvalidOperationException(
                "No se recibio autorizacion de administrador.");
        }

        await _api.PutAsync(
            $"/api/Caja/recibos/{reciboId}/anular",
            new
            {
                Motivo = motivo.Trim()
            },
            sesion.Token);
    }
}

public sealed class SesionAnulacionRespuesta
{
    public string Token { get; set; } = "";
}