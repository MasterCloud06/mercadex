using Mercadex.Application.Interfaces;
using Mercadex.Application.Services;
using Mercadex.Infrastructure.Persistence;
using Mercadex.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

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


// Application + Infrastructure Services

// Socios
builder.Services.AddScoped<ISocioRepository, SocioRepository>();
builder.Services.AddScoped<ISocioService, SocioService>();

// Obligaciones
builder.Services.AddScoped<IObligacionRepository, ObligacionRepository>();
builder.Services.AddScoped<IObligacionService, ObligacionService>();

// Caja
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IReciboRepository, ReciboRepository>();
builder.Services.AddScoped<ICajaService, CajaService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();