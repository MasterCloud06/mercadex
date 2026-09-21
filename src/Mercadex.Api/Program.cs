using Mercadex.Application.Interfaces;
using Mercadex.Application.Services;
using Mercadex.Infrastructure.Persistence;
using Mercadex.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Mercadex.Application.Interfaces.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();


var connectionString =
    builder.Configuration.GetConnectionString(
        "MercadexDatabase");


if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró la cadena de conexión MercadexDatabase.");
}


builder.Services.AddDbContext<MercadexDbContext>(
    options =>
        options.UseNpgsql(connectionString));


// =====================================
// Application + Infrastructure Services
// =====================================


// Socios
builder.Services.AddScoped<
    ISocioRepository,
    SocioRepository>();

builder.Services.AddScoped<
    ISocioService,
    SocioService>();


// Obligaciones
builder.Services.AddScoped<
    IObligacionRepository,
    ObligacionRepository>();

builder.Services.AddScoped<
    IObligacionService,
    ObligacionService>();


// Generador de Cobranza
builder.Services.AddScoped<
    IUnidadComercialRepository,
    UnidadComercialRepository>();

builder.Services.AddScoped<
    GeneradorCobranzaService>();


// Caja
builder.Services.AddScoped<
    IPagoRepository,
    PagoRepository>();

builder.Services.AddScoped<
    IReciboRepository,
    ReciboRepository>();

builder.Services.AddScoped<
    ICajaService,
    CajaService>();

// Unidad de trabajo
builder.Services.AddScoped<
    IUnitOfWork,
    UnitOfWork>();


// Estadísticas Caja
builder.Services.AddScoped<
    IEstadisticaCajaRepository,
    EstadisticaCajaRepository>();

builder.Services.AddScoped<
    IEstadisticaCajaService,
    EstadisticaCajaService>();

builder.Services.AddScoped<ISocioRepository, SocioRepository>();
builder.Services.AddScoped<SocioService>();

// Asignación Socio Caseta
builder.Services.AddScoped<
    IAsignacionSocioCasetaRepository,
    AsignacionSocioCasetaRepository>();

builder.Services.AddScoped<
    IAsignacionSocioCasetaService,
    AsignacionSocioCasetaService>();


var app = builder.Build();

app.UseMiddleware<Mercadex.Api.Middleware.ExceptionMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();

app.MapControllers();


app.Run();