using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mercadex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearKardexCasetas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "kardex_casetas",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UnidadComercialId = table.Column<Guid>(type: "uuid", nullable: false),
                    SocioId = table.Column<Guid>(type: "uuid", nullable: true),
                    FechaIngreso = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    EstadoVida = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TipoDocumentoPropiedad = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    PropietarioAnterior = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    AporteSocioPagado = table.Column<bool>(type: "boolean", nullable: true),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kardex_casetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kardex_casetas_socios_SocioId",
                        column: x => x.SocioId,
                        principalTable: "socios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_kardex_casetas_unidades_comerciales_UnidadComercialId",
                        column: x => x.UnidadComercialId,
                        principalTable: "unidades_comerciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_kardex_casetas_SocioId",
                schema: "public",
                table: "kardex_casetas",
                column: "SocioId");

            migrationBuilder.CreateIndex(
                name: "IX_kardex_casetas_UnidadComercialId",
                schema: "public",
                table: "kardex_casetas",
                column: "UnidadComercialId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kardex_casetas",
                schema: "public");
        }
    }
}
