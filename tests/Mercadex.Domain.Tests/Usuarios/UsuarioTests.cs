using Mercadex.Domain.Caja;
using Mercadex.Domain.Usuarios;
using Xunit;

namespace Mercadex.Domain.Tests.Usuarios;

public sealed class UsuarioTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 13, 12, 0, 0, TimeSpan.Zero);

    private static Usuario Nuevo(RolUsuario rol = RolUsuario.Cajero) =>
        new("  Maria.Lopez ", "María López", rol, "huella", Ahora);

    [Fact]
    public void Usuario_nuevo_queda_activo_con_contrasena_temporal_y_nombre_normalizado()
    {
        var usuario = Nuevo();

        Assert.Equal("maria.lopez", usuario.NombreUsuario);
        Assert.True(usuario.Activo);
        Assert.True(usuario.DebeCambiarContrasena);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("maría")]
    [InlineData("con espacio")]
    public void Nombre_de_usuario_invalido_se_rechaza(string nombre)
    {
        Assert.Throws<ArgumentException>(() =>
            new Usuario(nombre, "Nombre Apellido", RolUsuario.Consulta, "huella", Ahora));
    }

    [Fact]
    public void Cambiar_contrasena_quita_la_temporal_y_cierra_sesiones()
    {
        var usuario = Nuevo();
        var version = usuario.VersionSesion;

        usuario.CambiarContrasena("otra");

        Assert.False(usuario.DebeCambiarContrasena);
        Assert.NotEqual(version, usuario.VersionSesion);
    }

    [Fact]
    public void Restablecer_desbloquea_y_vuelve_a_pedir_cambio()
    {
        var usuario = Nuevo();
        usuario.CambiarContrasena("propia");

        for (var i = 0; i < Usuario.IntentosPermitidos; i++)
            usuario.RegistrarFallo(Ahora);

        Assert.True(usuario.EstaBloqueado(Ahora));

        usuario.RestablecerContrasena("temporal");

        Assert.False(usuario.EstaBloqueado(Ahora));
        Assert.True(usuario.DebeCambiarContrasena);
    }

    [Fact]
    public void Bloqueo_vence_con_el_tiempo()
    {
        var usuario = Nuevo();

        for (var i = 0; i < Usuario.IntentosPermitidos; i++)
            usuario.RegistrarFallo(Ahora);

        Assert.True(usuario.EstaBloqueado(Ahora.AddMinutes(1)));
        Assert.False(usuario.EstaBloqueado(Ahora + Usuario.TiempoBloqueo + TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void Cambiar_rol_o_desactivar_cierra_sesiones()
    {
        var usuario = Nuevo();
        var version = usuario.VersionSesion;

        usuario.Actualizar("María López", RolUsuario.Administrador);
        Assert.NotEqual(version, usuario.VersionSesion);

        version = usuario.VersionSesion;
        usuario.Desactivar();
        Assert.False(usuario.Activo);
        Assert.NotEqual(version, usuario.VersionSesion);
    }

    [Fact]
    public void Recibo_guarda_quien_registro_el_cobro()
    {
        var recibo = new Recibo(
            1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Ahora,
            new[] { new DetalleRecibo(Guid.NewGuid(), Guid.NewGuid(), "Mensualidad", 100m, 2026, 10) });

        recibo.AsignarRegistradoPor("  María López ");

        Assert.Equal("María López", recibo.RegistradoPor);
    }
}
