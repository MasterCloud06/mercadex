using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mercadex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEgresosEnCierreYAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnuladoPor",
                table: "egresos",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistradoPor",
                table: "egresos",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CantidadEgresos",
                table: "cierres_caja",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "EgresosEfectivo",
                table: "cierres_caja",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnuladoPor",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "RegistradoPor",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "CantidadEgresos",
                table: "cierres_caja");

            migrationBuilder.DropColumn(
                name: "EgresosEfectivo",
                table: "cierres_caja");
        }
    }
}
