# Mercadex Backend

API de Mercadex (.NET 10, ASP.NET Core + EF Core + PostgreSQL). Publicada en Render
desde la rama `main` (Docker); la base de datos es el PostgreSQL de Render.

| Repositorio | Qué es |
|---|---|
| **mercadex-backend** (este) | API, dominio, migraciones y pruebas |
| [mercadex-desktop](https://github.com/MasterCloud06/mercadex-desktop) | Aplicación de escritorio de la caja |
| [mercadex-panel](https://github.com/MasterCloud06/mercadex-panel) | Panel web de caja |
| [mercadex-movil](https://github.com/MasterCloud06/mercadex-movil) | App de socios (Android / iOS) |

## Ramas

- `develop`: trabajo diario.
- `main`: producción. Cada merge a `main` despliega en Render.

## Estructura

- `src/Mercadex.Domain`, `src/Mercadex.Infrastructure`, `src/Mercadex.Api`
- `tests/Mercadex.Domain.Tests`
- `database/`: scripts SQL de apoyo para verificar antes de migrar

```powershell
dotnet build Mercadex.sln
dotnet test Mercadex.sln
```

Configuración de producción y desarrollo local: [PRODUCCION.md](PRODUCCION.md).
