using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using RaymiMusic.AppWeb.Services;
using RaymiMusic.Modelos;

namespace RaymiMusic.AppWeb.Controllers
{
    public class Favoritos : Controller
    {
        private readonly ISongService _songService;

        public Favoritos(ISongService songService)
        {
            _songService = songService;
        }

        // GET: Favoritos
        public async Task<IActionResult> Index()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound();

            var cancionesFavoritas = await _songService.GetLikesForUser(Guid.Parse(userId));
            return View(cancionesFavoritas.ToList()); // O puedes dejarlo como IEnumerable
        }
    }
}