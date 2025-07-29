using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaymiMusic.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Artistas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreArtistico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Biografia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UrlFotoPerfil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UrlFotoPortada = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artistas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailConfirmations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailConfirmations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Generos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ListasPublicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreadaPor = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListasPublicas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Planes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    DescargasMaximas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Albumes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaLanzamiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NombreArchivoPortada = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArtistaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albumes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albumes_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HashContrasena = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlanSuscripcionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UrlFotoPerfil = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Planes_PlanSuscripcionId",
                        column: x => x.PlanSuscripcionId,
                        principalTable: "Planes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Canciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duracion = table.Column<TimeSpan>(type: "time", nullable: false),
                    GeneroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArtistaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlbumId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Canciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Canciones_Albumes_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albumes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Canciones_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Canciones_Generos_GeneroId",
                        column: x => x.GeneroId,
                        principalTable: "Generos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Follow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArtistaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaSeguimiento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Follow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Follow_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Follow_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListasReproduccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsPublica = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListasReproduccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListasReproduccion_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NumeroDeTarjeta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlanSuscripcionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreTitular = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodigoSeguridad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pago_Planes_PlanSuscripcionId",
                        column: x => x.PlanSuscripcionId,
                        principalTable: "Planes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Pago_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Perfiles",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UrlFoto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfiles", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_Perfiles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CancionAlbum",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlbumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancionAlbum", x => x.id);
                    table.ForeignKey(
                        name: "FK_CancionAlbum_Albumes_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CancionAlbum_Canciones_CancionId",
                        column: x => x.CancionId,
                        principalTable: "Canciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Descargas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Descargas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Descargas_Canciones_CancionId",
                        column: x => x.CancionId,
                        principalTable: "Canciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistorialReproducciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaReproduccion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialReproducciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialReproducciones_Canciones_CancionId",
                        column: x => x.CancionId,
                        principalTable: "Canciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CancionesEnListas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListaReproduccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListaPublicaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancionesEnListas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CancionesEnListas_Canciones_CancionId",
                        column: x => x.CancionId,
                        principalTable: "Canciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CancionesEnListas_ListasPublicas_ListaPublicaId",
                        column: x => x.ListaPublicaId,
                        principalTable: "ListasPublicas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CancionesEnListas_ListasReproduccion_ListaReproduccionId",
                        column: x => x.ListaReproduccionId,
                        principalTable: "ListasReproduccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Albumes_ArtistaId",
                table: "Albumes",
                column: "ArtistaId");

            migrationBuilder.CreateIndex(
                name: "IX_CancionAlbum_AlbumId",
                table: "CancionAlbum",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_CancionAlbum_CancionId",
                table: "CancionAlbum",
                column: "CancionId");

            migrationBuilder.CreateIndex(
                name: "IX_Canciones_AlbumId",
                table: "Canciones",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Canciones_ArtistaId",
                table: "Canciones",
                column: "ArtistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Canciones_GeneroId",
                table: "Canciones",
                column: "GeneroId");

            migrationBuilder.CreateIndex(
                name: "IX_CancionesEnListas_CancionId",
                table: "CancionesEnListas",
                column: "CancionId");

            migrationBuilder.CreateIndex(
                name: "IX_CancionesEnListas_ListaPublicaId",
                table: "CancionesEnListas",
                column: "ListaPublicaId");

            migrationBuilder.CreateIndex(
                name: "IX_CancionesEnListas_ListaReproduccionId",
                table: "CancionesEnListas",
                column: "ListaReproduccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Descargas_CancionId",
                table: "Descargas",
                column: "CancionId");

            migrationBuilder.CreateIndex(
                name: "IX_Follow_ArtistaId",
                table: "Follow",
                column: "ArtistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Follow_UsuarioId",
                table: "Follow",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialReproducciones_CancionId",
                table: "HistorialReproducciones",
                column: "CancionId");

            migrationBuilder.CreateIndex(
                name: "IX_ListasReproduccion_UsuarioId",
                table: "ListasReproduccion",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Pago_PlanSuscripcionId",
                table: "Pago",
                column: "PlanSuscripcionId");

            migrationBuilder.CreateIndex(
                name: "IX_Pago_UsuarioId",
                table: "Pago",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PlanSuscripcionId",
                table: "Usuarios",
                column: "PlanSuscripcionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CancionAlbum");

            migrationBuilder.DropTable(
                name: "CancionesEnListas");

            migrationBuilder.DropTable(
                name: "Descargas");

            migrationBuilder.DropTable(
                name: "EmailConfirmations");

            migrationBuilder.DropTable(
                name: "Follow");

            migrationBuilder.DropTable(
                name: "HistorialReproducciones");

            migrationBuilder.DropTable(
                name: "Pago");

            migrationBuilder.DropTable(
                name: "Perfiles");

            migrationBuilder.DropTable(
                name: "ListasPublicas");

            migrationBuilder.DropTable(
                name: "ListasReproduccion");

            migrationBuilder.DropTable(
                name: "Canciones");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Albumes");

            migrationBuilder.DropTable(
                name: "Generos");

            migrationBuilder.DropTable(
                name: "Planes");

            migrationBuilder.DropTable(
                name: "Artistas");
        }
    }
}
