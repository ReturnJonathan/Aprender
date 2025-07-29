using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaymiMusic.AppWeb.Services;
using RaymiMusic.AppWeb.Models;
using System.Security.Claims;
using RaymiMusic.Modelos;

namespace RaymiMusic.AppWeb.Controllers
{
    [Authorize]
    public class PlayerController : Controller
    {
        private readonly ISongService _songService;
        private readonly IPlanesService _planesService;
        private readonly IWebHostEnvironment _env;
        private readonly IPlaylistService _playlistService;
        private readonly IAlbumsService _albumsService;
        public PlayerController(ISongService songService, IPlanesService planesService, IWebHostEnvironment env, IPlaylistService playlistService,IAlbumsService albumsService)
        {
            _songService = songService;
            _planesService = planesService;
            _env = env;
            _playlistService = playlistService;
            _albumsService = albumsService;
        }

        // GET: /Player/Play/{id}

        public async Task<IActionResult> Play(Guid id, Guid? playlistId = null, Guid? albumId = null, string? returnUrl = null)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound();

            var plan = await _planesService.ObtenerPlanUsuarioAsync(Guid.Parse(userId));

            if (plan?.Nombre == "Free")
            {
                var anunciosPath = Path.Combine(_env.WebRootPath, "anuncios");
                if (Directory.Exists(anunciosPath))
                {
                    var archivos = Directory.GetFiles(anunciosPath);
                    if (archivos.Length > 0)
                    {
                        var random = new Random();
                        var anuncioElegido = Path.GetFileName(archivos[random.Next(archivos.Length)]);
                        ViewBag.Anuncio = "/anuncios/" + anuncioElegido;
                    }
                }
            }

            var song = await _songService.GetByIdAsync(id);
            if (song == null) return NotFound();

            ViewBag.songId = song.Id;
            ViewBag.PlaylistId = playlistId;
            ViewBag.AlbumId = albumId;

            bool modoAleatorio = HttpContext.Session.GetString("ModoAleatorio") == "true";
            ViewBag.ModoAleatorio = modoAleatorio;
            HttpContext.Session.SetString("CancionActual", id.ToString());

            if (playlistId.HasValue)
            {
                var listaReproduccion = await _playlistService.GetByIdAsync(playlistId.Value);
                if (listaReproduccion == null) return NotFound();
                var lista = listaReproduccion.Canciones.ToList();

                if (modoAleatorio)
                {
                    var aleatoria = lista.Where(c => c.Id != id)
                                         .OrderBy(c => Guid.NewGuid())
                                         .FirstOrDefault();
                    ViewBag.CancionSiguiente = aleatoria?.Id;
                }
                else
                {
                    var indexActual = lista.FindIndex(c => c.Id == id);
                    ViewBag.CancionAnterior = indexActual > 0 ? (Guid?)lista[indexActual - 1].Id : null;
                    ViewBag.CancionSiguiente = indexActual < lista.Count - 1 ? (Guid?)lista[indexActual + 1].Id : null;
                }

                ViewBag.ListaCanciones = lista.Select(c => c.Id).ToList();
            }
            else if (albumId.HasValue)
            {
                var album = (await _albumsService.GetAlbumsAsync()).FirstOrDefault(a => a.Id == albumId);
                if (album == null || album.Canciones == null || !album.Canciones.Any()) return NotFound();

                var lista = album.Canciones.ToList();

                if (modoAleatorio)
                {
                    var aleatoria = lista.Where(c => c.Id != id)
                                         .OrderBy(c => Guid.NewGuid())
                                         .FirstOrDefault();
                    ViewBag.CancionSiguiente = aleatoria?.Id;
                }
                else
                {
                    var indexActual = lista.FindIndex(c => c.Id == id);
                    ViewBag.CancionAnterior = indexActual > 0 ? (Guid?)lista[indexActual - 1].Id : null;
                    ViewBag.CancionSiguiente = indexActual < lista.Count - 1 ? (Guid?)lista[indexActual + 1].Id : null;
                }

                ViewBag.ListaCanciones = lista.Select(c => c.Id).ToList();
            }
            else
            {
                var todas = (await _songService.GetAllAsync()).ToList();
                var aleatoria = todas.OrderBy(x => Guid.NewGuid()).FirstOrDefault(x => x.Id != id);
                ViewBag.CancionSiguiente = aleatoria?.Id;
            }

            ViewBag.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Index", "Home") : returnUrl;

            return View(song);
        }
        [HttpGet]
        public IActionResult Error ()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ToggleAleatorioAjax()
        {
            var actual = HttpContext.Session.GetString("ModoAleatorio") == "true";
            HttpContext.Session.SetString("ModoAleatorio", (!actual).ToString().ToLower());

            return Json(new { estado = !actual });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike([FromBody] Guid idCancion)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return NotFound();

            var like = await _songService.GetSongLike(Guid.Parse(userId), idCancion);
            bool liked = false;

            if (like != null)
            {
                await _songService.DeleteLike(like.Id);
            }
            else
            {
                var newLike = new LikeCancion
                {
                    UsuarioId = Guid.Parse(userId),
                    CancionId = idCancion,
                    FechaCreacion = DateTime.UtcNow
                };
                await _songService.CreateLikeAsync(newLike);
                liked = true;
            }

            return Json(new { liked });
        }


    }
}
