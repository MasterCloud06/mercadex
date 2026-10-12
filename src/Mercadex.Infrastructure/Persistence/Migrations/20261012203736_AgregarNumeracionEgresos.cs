using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mercadex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNumeracionEgresos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "numeracion_egresos");

            migrationBuilder.AddColumn<long>(
                name: "Numero",
                table: "egresos",
                type: "bigint",
                nullable: false,
                defaultValueSql: "nextval('numeracion_egresos')");

            migrationBuilder.CreateIndex(
                name: "IX_egresos_Numero",
                table: "egresos",
                column: "Numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_egresos_Numero",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "egresos");

            migrationBuilder.DropSequence(
                name: "numeracion_egresos");
        }
    }
}
