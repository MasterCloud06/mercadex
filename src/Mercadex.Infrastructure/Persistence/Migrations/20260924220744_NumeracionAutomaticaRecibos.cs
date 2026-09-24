
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mercadex.Infrastructure.Persistence.Migrations
{
    public partial class NumeracionAutomaticaRecibos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE SEQUENCE IF NOT EXISTS numeracion_recibos
                AS bigint
                START WITH 1
                INCREMENT BY 1;
                """);

            migrationBuilder.Sql("""
                SELECT setval(
                    'numeracion_recibos',
                    GREATEST(
                        COALESCE(
                            (SELECT MAX("Numero") FROM recibos),
                            0
                        ) + 1,
                        1
                    ),
                    false
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP SEQUENCE IF EXISTS numeracion_recibos;
                """);
        }
    }
}
