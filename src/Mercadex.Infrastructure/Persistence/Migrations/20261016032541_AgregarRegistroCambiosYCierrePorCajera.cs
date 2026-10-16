using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mercadex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRegistroCambiosYCierrePorCajera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cierres_caja_Fecha_MonedaId",
                table: "cierres_caja");

            migrationBuilder.AddColumn<string>(
                name: "RegistradoPorId",
                table: "recibos",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistradoPorId",
                table: "egresos",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CajeroId",
                table: "cierres_caja",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "general");

            migrationBuilder.AddColumn<string>(
                name: "CajeroNombre",
                table: "cierres_caja",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "registro_cambios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Usuario = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Rol = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Origen = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Modulo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Accion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Detalle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Referencia = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registro_cambios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cierres_caja_Fecha_MonedaId_CajeroId",
                table: "cierres_caja",
                columns: new[] { "Fecha", "MonedaId", "CajeroId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_registro_cambios_FechaUtc",
                table: "registro_cambios",
                column: "FechaUtc");

            migrationBuilder.CreateIndex(
                name: "IX_registro_cambios_UsuarioId",
                table: "registro_cambios",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registro_cambios");

            migrationBuilder.DropIndex(
                name: "IX_cierres_caja_Fecha_MonedaId_CajeroId",
                table: "cierres_caja");

            migrationBuilder.DropColumn(
                name: "RegistradoPorId",
                table: "recibos");

            migrationBuilder.DropColumn(
                name: "RegistradoPorId",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "CajeroId",
                table: "cierres_caja");

            migrationBuilder.DropColumn(
                name: "CajeroNombre",
                table: "cierres_caja");

            migrationBuilder.CreateIndex(
                name: "IX_cierres_caja_Fecha_MonedaId",
                table: "cierres_caja",
                columns: new[] { "Fecha", "MonedaId" },
                unique: true);
        }
    }
}
