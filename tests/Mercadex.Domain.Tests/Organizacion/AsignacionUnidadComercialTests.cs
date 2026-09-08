using Mercadex.Domain.Organizacion;

namespace Mercadex.Domain.Tests.Organizacion;

public sealed class AsignacionUnidadComercialTests
{
    [Fact]
    public void NuevaAsignacion_DebeEstarVigente()
    {
        var asignacion =
            new AsignacionUnidadComercial(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                TipoVinculoUnidad.Titular,
                new DateOnly(2026, 9, 1));

        Assert.True(asignacion.EstaVigente);
        Assert.Null(asignacion.FechaFin);
    }

    [Fact]
    public void FinalizarAsignacion_DebeGuardarFecha()
    {
        var asignacion =
            new AsignacionUnidadComercial(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                TipoVinculoUnidad.Titular,
                new DateOnly(2026, 1, 1));

        asignacion.Finalizar(
            new DateOnly(2026, 9, 1));

        Assert.False(asignacion.EstaVigente);

        Assert.Equal(
            new DateOnly(2026, 9, 1),
            asignacion.FechaFin);
    }

    [Fact]
    public void FinalizarAntesDelInicio_DebeFallar()
    {
        var asignacion =
            new AsignacionUnidadComercial(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                TipoVinculoUnidad.Titular,
                new DateOnly(2026, 9, 1));

        Assert.Throws<ArgumentException>(() =>
            asignacion.Finalizar(
                new DateOnly(2026, 8, 31)));
    }
}
