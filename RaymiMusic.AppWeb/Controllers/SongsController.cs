using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using RaymiMusic.Api.Data;
using RaymiMusic.Modelos;
using RaymiMusic.AppWeb.Models;
using System.Globalization;

namespace RaymiMusic.AppWeb.Controllers
{
    [Authorize]
    public class SongsController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly IWebHostEnvironment _env;

        public SongsController(AppDbContext ctx, IWebHostEnvironment env)
        {
            _ctx = ctx;
            _env = env;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private bool IsAdmin => User.IsInRole(Roles.Admin);
        private bool IsArtist => User.IsInRole(Roles.Artista);

        // GET: /Songs
        public async Task<IActionResult> Index()
        {
            var query = _ctx.Canciones
                .Include(c => c.Artista)
                .Include(c => c.Genero)
                .Include(c => c.Album)
                .AsQueryable();

            if (IsArtist && !IsAdmin)
                query = query.Where(c => c.ArtistaId == CurrentUserId);

            var list = await query.AsNoTracking().ToListAsync();
            return View(list);
        }

        // GET: /Songs/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var c = await _ctx.Canciones
                .Include(c => c.Artista)
                .Include(c => c.Genero)
                .Include(c => c.Album)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (c == null) return NotFound();
            if (IsArtist && c.ArtistaId != CurrentUserId) return Forbid();
            return View(c);
        }

        // GET: /Songs/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new CancionCreateVM());
        }

        // POST: /Songs/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CancionCreateVM vm)
        {
            if (vm.AudioFile == null || vm.AudioFile.Length == 0)
            {
                ModelState.AddModelError(nameof(vm.AudioFile), "Debes seleccionar un archivo de audio.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(vm);
            }

            // Guardar archivo
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.AudioFile.FileName)}";
            var filePath = Path.Combine(uploads, fileName);
            await using (var stream = System.IO.File.Create(filePath))
                await vm.AudioFile.CopyToAsync(stream);

            // Extraer duración
            TimeSpan duracion;
            try
            {
                var tfile = TagLib.File.Create(filePath);
                duracion = tfile.Properties.Duration;
            }
            catch
            {
                ModelState.AddModelError("", "No se pudo leer la duración del MP3.");
                await PopulateDropdowns();
                return View(vm);
            }

            // Persistir entidad
            var entidad = new Cancion
            {
                Id = Guid.NewGuid(),
                Titulo = vm.Titulo,
                Duracion = duracion,
                GeneroId = vm.GeneroId,
                AlbumId = vm.AlbumId,
                RutaArchivo = $"/uploads/{fileName}",
                ArtistaId = IsAdmin
                                ? vm.ArtistaId!.Value
                                : CurrentUserId
            };

            _ctx.Canciones.Add(entidad);
            await _ctx.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: /Songs/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entidad = await _ctx.Canciones.FindAsync(id);
            if (entidad == null) return NotFound();
            if (!IsAdmin && entidad.ArtistaId != CurrentUserId) return Forbid();

            await PopulateDropdowns(entidad.ArtistaId);

            var vm = new CancionEditVM
            {
                Id = entidad.Id,
                Titulo = entidad.Titulo,
                GeneroId = entidad.GeneroId,
                AlbumId = entidad.AlbumId,
                ArtistaId = entidad.ArtistaId,
                DuracionActual = entidad.Duracion
            };
            return View(vm);
        }



        // POST: /Songs/Edit/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    Guid id,
    CancionEditVM vm)
        {
            if (id != vm.Id)
                return BadRequest();

            var entidad = await _ctx.Canciones.FindAsync(id);
            if (entidad == null)
                return NotFound();
            if (!IsAdmin && entidad.ArtistaId != CurrentUserId)
                return Forbid();

            // Si viene nuevo archivo, reemplazamos y recalculamos duración
            if (vm.AudioFile is { Length: > 0 })
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.AudioFile.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                await using var stream = System.IO.File.Create(filePath);
                await vm.AudioFile.CopyToAsync(stream);

                entidad.RutaArchivo = $"/uploads/{fileName}";
                var tfile = TagLib.File.Create(filePath);
                entidad.Duracion = tfile.Properties.Duration;
            }

            // Validamos el resto de datos
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(entidad.ArtistaId);
                return View(vm);
            }

            // Actualizamos metadatos
            entidad.Titulo = vm.Titulo;
            entidad.GeneroId = vm.GeneroId;
            entidad.AlbumId = vm.AlbumId;

            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }





        // GET: /Songs/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var c = await _ctx.Canciones
                .Include(c => c.Artista)
                .Include(c => c.Genero)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (c == null) return NotFound();
            if (IsArtist && c.ArtistaId != CurrentUserId) return Forbid();
            return View(c);
        }

        // POST: /Songs/Delete/{id}
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var c = await _ctx.Canciones.FindAsync(id);
            if (c == null) return NotFound();
            if (IsArtist && c.ArtistaId != CurrentUserId) return Forbid();

            _ctx.Canciones.Remove(c);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helpers

        private async Task PopulateDropdowns(Guid? artistaId = null)
        {
            ViewData["Generos"] = new SelectList(
                await _ctx.Generos.ToListAsync(), "Id", "Nombre");
            ViewData["Albums"] = new SelectList(
                await _ctx.Albumes.ToListAsync(), "Id", "Titulo");
            if (IsAdmin)
            {
                ViewData["Artistas"] = new SelectList(
                    await _ctx.Artistas.ToListAsync(),
                    "Id", "NombreArtistico",
                    artistaId);
            }
        }

        private async Task<IActionResult> ReloadEditView(Cancion c)
        {
            await PopulateDropdowns(c.ArtistaId);
            return View("Edit", c);
        }
    }
}
