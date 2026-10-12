using System.Threading.RateLimiting;
using Mercadex.Application.Interfaces;
using Mercadex.Application.Services;
using Mercadex.Infrastructure.Persistence;
using Mercadex.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Mercadex.Application.Interfaces.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();


// AUTENTICACIÓN PARA KARDEX

var jwtKey = builder.Configuration["MERCADEX_JWT_KEY"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Configure MERCADEX_JWT_KEY con al menos 32 bytes.");
}

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "Mercadex",

                ValidateAudience = true,
                ValidAudience = "Mercadex.Desktop",

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
    });

builder.Services.AddAuthorization();



var connectionString =
    builder.Configuration.GetConnectionString(
        "MercadexDatabase");


if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró la cadena de conexión MercadexDatabase.");
}


// MERCADEX 1.5 - PRODUCCION CONTROLADA
if (Environment.GetEnvironmentVariable("MERCADEX_ENABLE_V15") != "SI")
{
    throw new InvalidOperationException(
        "Falta habilitar MERCADEX_ENABLE_V15=SI.");
}

var destinoProduccion =
    new Npgsql.NpgsqlConnectionStringBuilder(connectionString);

if (!string.Equals(
        destinoProduccion.Database,
        "mercadex_db_4qha",
        StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "La base configurada no es la base Mercadex autorizada.");
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

// Ingresos

builder.Services.AddScoped<
    IIngresoRepository,
    IngresoRepository>();

builder.Services.AddScoped<
    IIngresoService,
    IngresoService>();

builder.Services.AddScoped<
    IAsignacionSocioCasetaService,
    AsignacionSocioCasetaService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?
                    .ToString() ?? "desconocida",

            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});


var app = builder.Build();

app.UseMiddleware<Mercadex.Api.Middleware.ExceptionMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();


app.Run();