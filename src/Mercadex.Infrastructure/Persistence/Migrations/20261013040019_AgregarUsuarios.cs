using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mercadex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegistradoPor",
                table: "recibos",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreUsuario = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    ContrasenaHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DebeCambiarContrasena = table.Column<bool>(type: "boolean", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    IntentosFallidos = table.Column<int>(type: "integer", nullable: false),
                    BloqueadoHastaUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VersionSesion = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacionUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UltimoAccesoUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_NombreUsuario",
                table: "usuarios",
                column: "NombreUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropColumn(
                name: "RegistradoPor",
                table: "recibos");
        }
    }
}
