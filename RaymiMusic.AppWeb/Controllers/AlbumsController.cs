using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RaymiMusic.Api.Data;
using RaymiMusic.Modelos;
using RaymiMusic.AppWeb.Models;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RaymiMusic.AppWeb.Controllers
{
    [Authorize]
    public class AlbumsController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly IWebHostEnvironment _env;

        public AlbumsController(AppDbContext ctx, IWebHostEnvironment env)
        {
            _ctx = ctx;
            _env = env;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private bool IsArtist => User.IsInRole("Artista");

        // GET: /Albums
        public async Task<IActionResult> Index()
        {
            var query = _ctx.Albumes
                            .Include(a => a.Artista)
                            .Include(a => a.CancionesAlbum)
                            .ThenInclude(ca => ca.Cancion) // Incluye las canciones del álbum
                            .AsQueryable();

            if (IsArtist)
                query = query.Where(a => a.ArtistaId == CurrentUserId);

            var list = await query.AsNoTracking().ToListAsync();
            return View(list);  // Redirige a la vista de álbumes
        }

        // GET: /Albums/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            // Cargo el álbum y el artista
            var album = await _ctx.Albumes
                                  .Include(a => a.Artista)
                                  .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
                return NotFound();

            // Traigo todas las canciones que tengan este AlbumId
            var canciones = await _ctx.Canciones
                                      .Where(c => c.AlbumId == id)
                                      .OrderBy(c => c.Id)
                                      .ToListAsync();

            // Le asigno la lista al álbum para que la vista la consuma
            album.Canciones = canciones;

            return View(album);
        }



        // GET: /Albums/Create
        public IActionResult Create()
        {
            var vm = new AlbumCreateVM();
            if (User.IsInRole("Admin"))
            {
                vm.Artistas = new SelectList(_ctx.Artistas, "Id", "NombreArtistico");
            }
            else
            {
                // Para artista, forzarle su propio Id
                vm.ArtistaId = CurrentUserId;
            }
            return View(vm);  // Redirige a la vista de creación de álbum
        }

        // POST: /Albums/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlbumCreateVM vm)
        {
            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                    vm.Artistas = new SelectList(_ctx.Artistas, "Id", "NombreArtistico", vm.ArtistaId);
                return View(vm);  // Si el modelo no es válido, redirige de nuevo al formulario
            }

            // Crear un nuevo álbum
            var album = new Album
            {
                Id = Guid.NewGuid(),
                Titulo = vm.Titulo,
                FechaLanzamiento = vm.FechaLanzamiento,
                ArtistaId = User.IsInRole("Admin") ? vm.ArtistaId : CurrentUserId
            };

            // Subir la portada
            if (vm.Portada != null)
            {
                // Crear un nombre único para el archivo de la portada
                var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(vm.Portada.FileName);
                var ruta = Path.Combine(_env.WebRootPath, "media/portadas", nombreArchivo);

                // Guardar la portada en la carpeta media/portadas
                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    await vm.Portada.CopyToAsync(stream);
                }

                // Guardar el nombre del archivo en el campo correspondiente
                album.NombreArchivoPortada = nombreArchivo;
            }

            // Guardar el álbum en la base de datos
            _ctx.Albumes.Add(album);
            await _ctx.SaveChangesAsync();

            return RedirectToAction(nameof(Index));  // Redirige al índice después de crear el álbum
        }


        // GET: /Albums/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var album = await _ctx.Albumes.FindAsync(id);
            if (album == null) return NotFound();

            var vm = new AlbumEditVM
            {
                Id = album.Id,
                Titulo = album.Titulo,
                FechaLanzamiento = album.FechaLanzamiento,
                ArtistaId = album.ArtistaId,
                Artistas = User.IsInRole("Admin")
                ? new SelectList(_ctx.Artistas, "Id", "NombreArtistico", album.ArtistaId)
                : null
            };
            return View(vm);  // Redirige a la vista de edición del álbum
        }

        // POST: /Albums/Edit/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AlbumEditVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Artistas = new SelectList(_ctx.Artistas, "Id", "NombreArtistico", vm.ArtistaId);
                return View(vm);  // Si hay errores en el formulario, redirige a la vista de edición
            }

            var album = await _ctx.Albumes.FindAsync(id);
            if (album == null) return NotFound();

            album.Titulo = vm.Titulo;
            album.FechaLanzamiento = vm.FechaLanzamiento;
            album.ArtistaId = vm.ArtistaId;

            // Subir nueva portada si es que la hay
            if (vm.Portada != null)
            {
                var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(vm.Portada.FileName);
                var ruta = Path.Combine(_env.WebRootPath, "media/portadas", nombreArchivo);
                using var stream = new FileStream(ruta, FileMode.Create);
                await vm.Portada.CopyToAsync(stream);
                album.NombreArchivoPortada = nombreArchivo;
            }

            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));  // Redirige al índice después de editar el álbum
        }

        // GET: /Albums/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var album = await _ctx.Albumes
                                  .Include(a => a.Artista)
                                  .FirstOrDefaultAsync(a => a.Id == id);
            if (album == null) return NotFound();
            return View(album);  // Redirige a la vista de eliminación del álbum
        }

        // POST: /Albums/Delete/{id}
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var album = await _ctx.Albumes.FindAsync(id);
            if (album != null)
            {
                _ctx.Albumes.Remove(album);
                await _ctx.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));  // Redirige al índice después de eliminar el álbum
        }
    }
}
