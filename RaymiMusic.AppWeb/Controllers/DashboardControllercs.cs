using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaymiMusic.Api.Data;
using RaymiMusic.AppWeb.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace RaymiMusic.AppWeb.Controllers
{
    [Authorize(Roles = "artista")]
    [Route("[controller]/[action]")]
    [Route("Artistas/[action]")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly IHttpClientFactory _clientFactory;

        public DashboardController(AppDbContext ctx, IHttpClientFactory clientFactory)
        {
            _ctx = ctx;
            _clientFactory = clientFactory;
        }

        // GET: /Artistas/Dashboard/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Dashboard(Guid id)
        {
            // 1) Recupero el artista
            var artista = await _ctx.Artistas
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
            if (artista == null)
                return NotFound("No se encontró el artista.");

            // 2) Totales
            var totalCanciones = await _ctx.Canciones.CountAsync(c => c.ArtistaId == artista.Id);
            var totalAlbumes = await _ctx.Albumes.CountAsync(a => a.ArtistaId == artista.Id);
            var totalPlaylists = await _ctx.ListasPublicas.CountAsync(p => p.CreadaPor == artista.NombreArtistico);

            // 3) Últimas canciones
            var ultimas = await _ctx.Canciones
                .Where(c => c.ArtistaId == artista.Id)
                .OrderByDescending(c => c.Id)
                .Take(5)
                .Select(c => new SongDTO
                {
                    Id = c.Id,
                    Titulo = c.Titulo,
                    AlbumNombre = c.Album != null ? c.Album.Titulo : "—",
                    ArtistaNombre = artista.NombreArtistico,
                    Duracion = c.Duracion,
                    RutaArchivo = c.RutaArchivo
                })
                .ToListAsync();

            // 4) Reproducciones
            var cancionesConReps = await _ctx.Canciones
                .Where(c => c.ArtistaId == artista.Id)
                .Select(c => new SongDTO
                {
                    Id = c.Id,
                    Titulo = c.Titulo,
                    AlbumNombre = c.Album != null ? c.Album.Titulo : "—",
                    ArtistaNombre = artista.NombreArtistico,
                    Duracion = c.Duracion,
                    RutaArchivo = c.RutaArchivo,
                    TotalReproducciones = _ctx.HistorialReproducciones.Count(r => r.CancionId == c.Id)
                })
                .ToListAsync();

            // 5) Descargas: llamar a la API
            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7153/");

            var cancionesConDescargas = new List<SongDTO>();
            foreach (var c in ultimas) // o cancionesConReps si prefieres
            {
                int totalDescargas = 0;
                try
                {
                    var response = await client.GetAsync($"api/Descargas/total/{c.Id}");

                    if (response.IsSuccessStatusCode)
                    {
                        var str = await response.Content.ReadAsStringAsync();
                        int.TryParse(str, out totalDescargas);
                    }
                }
                catch { /* ignorar errores por ahora */ }

                cancionesConDescargas.Add(new SongDTO
                {
                    Titulo = c.Titulo,
                    TotalDescargas = totalDescargas
                });
            }

            // 6) ViewModel
            var vm = new ArtistDashboardVM
            {
                NombreArtista = artista.NombreArtistico,
                TotalCanciones = totalCanciones,
                TotalAlbumes = totalAlbumes,
                TotalPlaylists = totalPlaylists,
                UltimasCanciones = ultimas,
                CancionesConReproducciones = cancionesConReps,
                CancionesConDescargas = cancionesConDescargas
            };

            return View(vm);
        }
    }
}
