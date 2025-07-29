using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RaymiMusic.Api.Data;
using RaymiMusic.AppWeb.Services;
using RaymiMusic.Modelos;       // ← contiene Cancion, Artista, Genero y RaymiMusicContext
using System.IO;
using System.Security.Claims;

namespace RaymiMusic.AppWeb.Controllers
{

    public class CancionesController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly IPlanesService _planesService;
        public CancionesController(AppDbContext ctx,IPlanesService planesService)
        {
            _ctx = ctx;
            _planesService = planesService;
        }
        /*------------------------------------------------------------------
         * LISTADO
         *----------------------------------------------------------------*/
        // GET: /Canciones
        public async Task<IActionResult> Index()
        {
            var data = await _ctx.Canciones
                                 .Include(c => c.Artista)
                                 .Include(c => c.Genero)
                                 .AsNoTracking()
                                 .ToListAsync();
            return View(data);
        }

        /*------------------------------------------------------------------
         * DETALLES
         *----------------------------------------------------------------*/
        // GET: /Canciones/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var cancion = await _ctx.Canciones
                                    .Include(c => c.Artista)
                                    .Include(c => c.Genero)
                                    .FirstOrDefaultAsync(c => c.Id == id);

            return cancion == null ? NotFound() : View(cancion);
        }

        /*------------------------------------------------------------------
         * CREAR
         *----------------------------------------------------------------*/
        // GET: /Canciones/Create
        public IActionResult Create()
        {
            SetDropDowns();
            return View();
        }

        // POST: /Canciones/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Titulo,Duracion,ArtistaId,GeneroId")] Cancion cancion,
            IFormFile archivoMp3)
        {
            if (archivoMp3 == null)
                ModelState.AddModelError("", "Debes seleccionar un archivo MP3.");

            if (!ModelState.IsValid)
            {
                SetDropDowns(cancion.ArtistaId, cancion.GeneroId);
                return View(cancion);
            }

            // 1) Validar y guardar archivo
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsPath);

            var ext = Path.GetExtension(archivoMp3.FileName).ToLower();
            if (ext != ".mp3")
            {
                ModelState.AddModelError("", "Solo se permiten archivos MP3.");
                SetDropDowns(cancion.ArtistaId, cancion.GeneroId);
                return View(cancion);
            }

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsPath, fileName);

            await using (var stream = System.IO.File.Create(fullPath))
                await archivoMp3.CopyToAsync(stream);

            // 2) Persistir entidad
            cancion.Id = Guid.NewGuid();
            cancion.RutaArchivo = $"/uploads/{fileName}";
            _ctx.Canciones.Add(cancion);
            await _ctx.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        /*------------------------------------------------------------------
         * EDITAR
         *----------------------------------------------------------------*/
        // GET: /Canciones/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var cancion = await _ctx.Canciones.FindAsync(id);
            if (cancion == null) return NotFound();

            SetDropDowns(cancion.ArtistaId, cancion.GeneroId, cancion.AlbumId);
            return View(cancion);
        }

        // POST: /Canciones/Edit/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            [Bind("Id,Titulo,Duracion,ArtistaId,GeneroId,AlbumId")] Cancion cancion,
            IFormFile? nuevoArchivo)  // Campo para subir un nuevo archivo (opcional)
        {
            if (id != cancion.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                SetDropDowns(cancion.ArtistaId, cancion.GeneroId);
                return View(cancion);
            }

            var entidad = await _ctx.Canciones.FindAsync(id);
            if (entidad == null) return NotFound();

            // Si se sube un nuevo archivo, reemplazar
            if (nuevoArchivo != null && nuevoArchivo.Length > 0)
            {
                var ext = Path.GetExtension(nuevoArchivo.FileName).ToLower();
                if (ext != ".mp3")
                {
                    ModelState.AddModelError("", "Solo se permiten archivos MP3.");
                    SetDropDowns(cancion.ArtistaId, cancion.GeneroId);
                    return View(cancion);
                }

                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                var fileName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(uploadsPath, fileName);

                Directory.CreateDirectory(uploadsPath);
                await using (var stream = System.IO.File.Create(fullPath))
                    await nuevoArchivo.CopyToAsync(stream);

                // Eliminar el archivo anterior si existía
                if (!string.IsNullOrWhiteSpace(entidad.RutaArchivo))
                {
                    var antiguo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", entidad.RutaArchivo.TrimStart('/'));
                    if (System.IO.File.Exists(antiguo))
                        System.IO.File.Delete(antiguo);
                }

                // Guardamos la nueva ruta del archivo
                entidad.RutaArchivo = $"/uploads/{fileName}";
            }

            // Actualizamos los campos de la canción
            entidad.Titulo = cancion.Titulo;
            entidad.Duracion = cancion.Duracion;
            entidad.ArtistaId = cancion.ArtistaId;
            entidad.GeneroId = cancion.GeneroId;
            entidad.AlbumId = cancion.AlbumId;

            _ctx.Update(entidad);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /*------------------------------------------------------------------
         * ELIMINAR
         *----------------------------------------------------------------*/
        // GET: /Canciones/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var cancion = await _ctx.Canciones
                                    .Include(c => c.Artista)
                                    .FirstOrDefaultAsync(c => c.Id == id);

            return cancion == null ? NotFound() : View(cancion);
        }

        // POST: /Canciones/Delete/{id}
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cancion = await _ctx.Canciones.FindAsync(id);
            if (cancion != null)
            {
                /* eliminar archivo físico */
                if (!string.IsNullOrWhiteSpace(cancion.RutaArchivo))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cancion.RutaArchivo.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _ctx.Canciones.Remove(cancion);
                await _ctx.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: /Canciones/Descargar/{id}
        [HttpGet]
        public async Task<IActionResult> Descargar(Guid id)
        {
            var cancion = await _ctx.Canciones.FindAsync(id);
            if (cancion == null)
                return NotFound();

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound();

            var plan = await _planesService.ObtenerPlanUsuarioAsync(Guid.Parse(userId));

            if (plan?.Nombre == "Free")
            {
                
                return RedirectToAction("Error", "Player", new { message = "Debes tener un plan Premium para descargar canciones." });



            }

            // REGISTRAR DESCARGA usando la API
            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7153/");

            try
            {
                var result = await client.PostAsJsonAsync("api/Descargas/registrar", new { CancionId = id });
                if (!result.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error registrando descarga");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al registrar descarga: {ex.Message}");
            }

            //DEVOLVER ARCHIVO MP3
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cancion.RutaArchivo.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
                return NotFound("Archivo no encontrado.");

            var mime = "audio/mpeg";
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            string safeTitulo = string.Concat(cancion.Titulo.Split(Path.GetInvalidFileNameChars()));
            return File(fileStream, mime, $"{safeTitulo}.mp3");

           // return File(fileStream, mime, Path.GetFileName(filePath));
        }

        /*------------------------------------------------------------------
         * HELPER: combos desplegables
         *----------------------------------------------------------------*/
        private void SetDropDowns(Guid? artistaId = null, Guid? generoId = null, Guid? albumId = null)
        {
            ViewBag.Artistas = new SelectList(_ctx.Artistas, "Id", "NombreArtistico", artistaId);
            ViewBag.Generos = new SelectList(_ctx.Generos, "Id", "Nombre", generoId);
            ViewBag.Albums = new SelectList(_ctx.Albumes, "Id", "Titulo", albumId);
        }
    }
}
