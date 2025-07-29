using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaymiMusic.Api.Data;
using RaymiMusic.Modelos;

namespace RaymiMusic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlanesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PlanesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Planes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlanSuscripcion>>> GetPlanes()
        {
            return await _context.Planes
                                 .Include(p => p.Usuarios)   // Navegación inversa
                                 .ToListAsync();
        }


        // GET: api/Planes/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PlanSuscripcion>> GetPlan(Guid id)
        {
            var plan = await _context.Planes
                                     .Include(p => p.Usuarios)
                                     .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null) return NotFound();
            return plan;
        }
        // GET: api/Planes/{id}
        [HttpGet("Nombre/{nombrePlan}")]
        public async Task<ActionResult<PlanSuscripcion>> GetPlanByNombre(string nombrePlan)
        {
            var plan = await _context.Planes
                                     .FirstOrDefaultAsync(p => p.Nombre == nombrePlan);

            if (plan == null) return NotFound();
            return plan;
        }
        [HttpGet("User/{userId:guid}")]
        public async Task<ActionResult<PlanSuscripcion>> GetPlanByUser(Guid userId)
        {
            var plan = await _context.Planes
                                     .Include(p => p.Usuarios)
                                     .SingleOrDefaultAsync(p => p.Usuarios.Any(u => u.Id == userId));

            if (plan == null)
                return NotFound("El usuario no tiene un plan de suscripción.");

            return Ok(plan);
        }


        // POST: api/Planes
        [HttpPost]
        public async Task<ActionResult<PlanSuscripcion>> PostPlan(PlanSuscripcion plan)
        {
            plan.Id = Guid.NewGuid();
            _context.Planes.Add(plan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlan), new { id = plan.Id }, plan);
        }

        // PUT: api/Planes/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> PutPlan(Guid id, PlanSuscripcion plan)
        {
            if (id != plan.Id) return BadRequest();

            _context.Entry(plan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                bool exists = await _context.Planes.AnyAsync(p => p.Id == id);
                if (!exists) return NotFound();
                throw;
            }

            return NoContent();
        }
        [HttpPost("Asignar/{userId:Guid}/{nombrePlan}")]
        public async Task AsignarPlan(Guid userId, string nombrePlan)
        {
            var plan = await _context.Planes
                                     .FirstOrDefaultAsync(p => p.Nombre == nombrePlan);
            if (plan == null) throw new Exception("Plan no encontrado");
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null) throw new Exception("Usuario no encontrado");
            usuario.PlanSuscripcion = plan;
            if (usuario.Rol == "Artista" || usuario.Rol == "Admin")
            {
                return; // No cambiar el rol de artistas o admins
            }
            if (plan.Nombre == "Free")
            {
                usuario.Rol = "free";
            }
            if (plan.Nombre != "Free")
            {
                usuario.Rol = "premium";
            }
            await _context.SaveChangesAsync();

        }
        // DELETE: api/Planes/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePlan(Guid id)
        {
            var plan = await _context.Planes.FindAsync(id);
            if (plan == null) return NotFound();

            _context.Planes.Remove(plan);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
