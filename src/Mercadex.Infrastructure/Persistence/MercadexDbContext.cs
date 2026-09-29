using Mercadex.Infrastructure.Persistence.Entities;
using Mercadex.Domain.Caja;
using Mercadex.Domain.Cobranza;
using Mercadex.Domain.Organizacion;
using Mercadex.Domain.Socios;
using Mercadex.Domain.Comercial;
using Microsoft.EntityFrameworkCore;
using Mercadex.Domain.Finanzas;

namespace Mercadex.Infrastructure.Persistence;

public sealed class MercadexDbContext : DbContext
{
    public MercadexDbContext(
        DbContextOptions<MercadexDbContext> options)
        : base(options)
    {
    }


    public DbSet<Socio> Socios =>
        Set<Socio>();


    public DbSet<Pasillo> Pasillos =>
        Set<Pasillo>();


    public DbSet<UnidadComercial> UnidadesComerciales =>
        Set<UnidadComercial>();


    public DbSet<AsignacionUnidadComercial> AsignacionesUnidades =>
        Set<AsignacionUnidadComercial>();


    public DbSet<AsignacionSocioCaseta> AsignacionesSocioCaseta =>
        Set<AsignacionSocioCaseta>();


    public DbSet<ConceptoCobro> ConceptosCobro =>
        Set<ConceptoCobro>();


    public DbSet<Moneda> Monedas =>
        Set<Moneda>();


    public DbSet<Obligacion> Obligaciones =>
        Set<Obligacion>();


    public DbSet<Pago> Pagos =>
        Set<Pago>();


    public DbSet<Recibo> Recibos =>
        Set<Recibo>();


    public DbSet<DetalleRecibo> DetallesRecibo =>
        Set<DetalleRecibo>();


    public DbSet<KardexCaseta> KardexCasetas =>
        Set<KardexCaseta>();

public DbSet<Egreso> Egresos =>
    Set<Egreso>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MercadexDbContext).Assembly);
    }
}
