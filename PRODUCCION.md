# Mercadex – Producción vs. desarrollo

## Cómo está organizado

| | Producción | Desarrollo local |
|---|---|---|
| API | Render: `https://mercadex-api.onrender.com` (Docker, rama `main`) | `dotnet run` en `src/Mercadex.Api` |
| Base de datos | PostgreSQL de Render | PostgreSQL local |
| Desktop | `appsettings.json` → URL de Render | Variable de entorno `MERCADEX_API_URL=http://127.0.0.1:5189/` |

Ya **no** hay bloqueos en el código que dependan de `MERCADEX_TARIFAS_DEV_MODE`,
`MERCADEX_ENABLE_V15`, del puerto `5189` ni del nombre de la base. La separación
entre entornos se hace solo con configuración.

## Variables de entorno de la API en Render

| Variable | Obligatoria | Valor |
|---|---|---|
| `ConnectionStrings__MercadexDatabase` **o** `DATABASE_URL` | Sí | Usar la *Internal Database URL* de Render (formato `postgresql://...` se convierte solo) |
| `MERCADEX_JWT_KEY` | Sí | Cadena aleatoria de 32+ caracteres |
| `MERCADEX_ADMIN_USER` | Sí (login Kardex/Tarifas/Reportes) | Usuario administrador |
| `MERCADEX_ADMIN_PASSWORD` | Sí | Contraseña del administrador |
| `MERCADEX_APLICAR_MIGRACIONES` | Recomendado | `true` → aplica migraciones al iniciar |
| `MERCADEX_KARDEX_STORAGE_PATH` | Para documentos Kardex | Ruta absoluta de un *Persistent Disk* (p. ej. `/var/data/kardex`) |

Variables que ya **no** se usan y se pueden borrar: `MERCADEX_ENABLE_V15`, `MERCADEX_TARIFAS_DEV_MODE`.

> El disco del contenedor de Render se borra en cada despliegue. Sin un
> Persistent Disk, los archivos subidos al Kardex se pierden.

## Desplegar Mercadex 1.5 a producción

1. Respaldar la base (Render → base de datos → *Backups*).
2. Ejecutar `database/VerificarAntesDeMigrar150.sql` contra la base de producción.
   La primera consulta **no** debe devolver filas.
3. Configurar las variables de arriba en el servicio `mercadex-api`.
4. Hacer merge de la rama de corrección a `main` → Render despliega.
5. Abrir `https://mercadex-api.onrender.com/api/health` y comprobar
   `"database.disponible": true` y `"migrations.pendientes": 0`.
6. Recién entonces publicar el instalador Desktop 1.5.0
   (`mercadex-updates/version.json`). Si se publica antes, los clientes
   se actualizan a una versión que llama endpoints que la API todavía no tiene.

## Desarrollo local

```powershell
# API
cd src/Mercadex.Api
dotnet user-secrets set "ConnectionStrings:MercadexDatabase" "Host=127.0.0.1;Port=5432;Database=mercadex_dev;Username=...;Password=..."
dotnet user-secrets set "MERCADEX_JWT_KEY" "<32+ caracteres>"
dotnet user-secrets set "MERCADEX_ADMIN_USER" "admin"
dotnet user-secrets set "MERCADEX_ADMIN_PASSWORD" "..."
dotnet run --urls http://127.0.0.1:5189

# Desktop apuntando a la API local
$env:MERCADEX_API_URL = "http://127.0.0.1:5189/"
dotnet run --project Desktop/Mercadex.Desktop.App
```

Nunca poner credenciales de la base de producción en `appsettings*.json` ni en el repositorio.
