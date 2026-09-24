
using Mercadex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Mercadex.Infrastructure.Persistence.Migrations;

[DbContext(typeof(MercadexDbContext))]
[Migration("20260924220000_AsegurarDocumentoSocioUnico")]
public sealed class AsegurarDocumentoSocioUnico : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS
                "UX_socios_DocumentoNormalizado"
            ON public.socios (
                UPPER(TRIM("DocumentoIdentidad"))
            );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP INDEX IF EXISTS
                public."UX_socios_DocumentoNormalizado";
            """);
    }
}
