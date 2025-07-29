using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaymiMusic.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLikeCancionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LikeCancion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LikeCancion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LikeCancion_Canciones_CancionId",
                        column: x => x.CancionId,
                        principalTable: "Canciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LikeCancion_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LikeCancion_CancionId",
                table: "LikeCancion",
                column: "CancionId");

            migrationBuilder.CreateIndex(
                name: "IX_LikeCancion_UsuarioId",
                table: "LikeCancion",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LikeCancion");
        }
    }
}
