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



// =====================================
// Base de datos
// =====================================
// Produccion (Render): variable de entorno
//   ConnectionStrings__MercadexDatabase
// o, como alternativa, DATABASE_URL (postgresql://usuario:clave@host:puerto/base).
// Desarrollo local: dotnet user-secrets o appsettings.Development.json
// (nunca subir credenciales al repositorio).

var connectionString =
    builder.Configuration.GetConnectionString("MercadexDatabase");

if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = ConvertirDatabaseUrl(
        builder.Configuration["DATABASE_URL"]);
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró la cadena de conexión. Configure " +
        "ConnectionStrings__MercadexDatabase o DATABASE_URL.");
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


builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(
    options =>
    {
        // Render termina HTTPS en su proxy y reenvia la peticion por HTTP.
        options.ForwardedHeaders =
            Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
            Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });


var app = builder.Build();

app.UseForwardedHeaders();


// Migraciones automaticas (opcional).
// Activar con MERCADEX_APLICAR_MIGRACIONES=true para que la API
// actualice el esquema de la base al iniciar.
if (app.Configuration.GetValue<bool>("MERCADEX_APLICAR_MIGRACIONES"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider
        .GetRequiredService<MercadexDbContext>();
    db.Database.Migrate();
}

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


static string? ConvertirDatabaseUrl(string? databaseUrl)
{
    if (string.IsNullOrWhiteSpace(databaseUrl))
        return null;

    if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri) ||
        (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
    {
        // Ya viene en formato Npgsql (Host=...;Database=...)
        return databaseUrl;
    }

    var credenciales = uri.UserInfo.Split(':', 2);

    var cadena = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(credenciales[0]),
        Password = credenciales.Length > 1
            ? Uri.UnescapeDataString(credenciales[1])
            : null,
        SslMode = Npgsql.SslMode.Require
    };

    return cadena.ConnectionString;
}
