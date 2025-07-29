using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RaymiMusic.Modelos;
namespace RaymiMusic.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts)
        : base(opts) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Perfil> Perfiles { get; set; }
        public DbSet<Artista> Artistas { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<PlanSuscripcion> Planes { get; set; }
        public DbSet<Cancion> Canciones { get; set; }
        public DbSet<Album> Albumes { get; set; }
        public DbSet<ListaReproduccion> ListasReproduccion { get; set; }
        public DbSet<CancionLista> CancionesEnListas { get; set; }
        public DbSet<ListaPublica> ListasPublicas { get; set; }
        public DbSet<RaymiMusic.Modelos.HistorialReproducciones> HistorialReproducciones { get; set; } = default!;
        public DbSet<Descarga> Descargas { get; set; }
        public DbSet<CancionAlbum> CancionAlbum { get; set; }
        public DbSet<EmailConfirmation> EmailConfirmations { get; set; }
        public DbSet<RaymiMusic.Modelos.Follow> Follow { get; set; } = default!;
        public DbSet<RaymiMusic.Modelos.Pago> Pago { get; set; } = default!;

        public DbSet<RaymiMusic.Modelos.LikeCancion> LikeCancion { get; set; }
    }
}
