using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RaymiMusic.Api.Data;
using RaymiMusic.Modelos;

namespace RaymiMusic.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikesCancionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LikesCancionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/LikesCanciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeCancion>>> GetLikeCancion()
        {

            return await _context.LikeCancion.ToListAsync();
        }

        // GET: api/LikesCanciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LikeCancion>> GetLikeCancion(Guid id)
        {
            var likeCancion = await _context.LikeCancion.FindAsync(id);

            if (likeCancion == null)
            {
                return NotFound();
            }

            return likeCancion;
        }

        // PUT: api/LikesCanciones/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLikeCancion(Guid id, LikeCancion likeCancion)
        {
            if (id != likeCancion.Id)
            {
                return BadRequest();
            }

            _context.Entry(likeCancion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LikeCancionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/LikesCanciones
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LikeCancion>> PostLikeCancion(LikeCancion likeCancion)
        {
            _context.LikeCancion.Add(likeCancion);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLikeCancion", new { id = likeCancion.Id }, likeCancion);
        }

        // DELETE: api/LikesCanciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLikeCancion(Guid id)
        {
            var likeCancion = await _context.LikeCancion.FindAsync(id);
            if (likeCancion == null)
            {
                return NotFound();
            }

            _context.LikeCancion.Remove(likeCancion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LikeCancionExists(Guid id)
        {
            return _context.LikeCancion.Any(e => e.Id == id);
        }
        // GET: api/LikesCanciones/usuario/{usuarioId}
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<LikeCancion>>> GetLikesByUsuario(Guid usuarioId)
        {
            var likes = await _context.LikeCancion
                .Where(l => l.UsuarioId == usuarioId)
                .Include(l => l.Cancion)
                    .ThenInclude(c => c.Artista)
                .Include(l => l.Cancion)
                    .ThenInclude(c => c.Album)
                .ToListAsync();

            return Ok(likes); // Siempre devuelve 200 con array vacío si no hay likes
        }

        // GET: api/LikesCanciones/usuario/{usuarioId}/cancion/{cancionId}
        [HttpGet("usuario/{usuarioId}/cancion/{cancionId}")]
        public async Task<ActionResult<LikeCancion>> GetLike(Guid usuarioId, Guid cancionId)
        {
            // Buscar la suscripción con los Ids proporcionados
            var like = await _context.LikeCancion
                                        .FirstOrDefaultAsync(LikeCancion => LikeCancion.UsuarioId == usuarioId && LikeCancion.CancionId == cancionId);

            if (like == null)
            {
                return NotFound();
            }

            return like;
        }

    }
}