using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using apirestfull.Data;
using apirestfull.Models;

namespace apirestfull.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Se inyecta el contexto para poder acceder a la base de datos
        public LoginsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /logins
        // Devuelve todos los registros de logins y logouts
        [HttpGet]
        public async Task<IActionResult> GetLogins()
        {
            var data = await _context.Logins.ToListAsync();
            return Ok(data);
        }

        // POST /logins
        // Registra un nuevo login o logout con validaciones
        [HttpPost]
        public async Task<IActionResult> Create(Login login)
        {
            // Validar que el usuario exista en la tabla Users
            var userExists = await _context.Users.AnyAsync(u => u.User_id == login.User_id);
            if (!userExists)
                return BadRequest("El User_id no existe");

            // Validar que la fecha no sea invalida
            if (login.Fecha > DateTime.Now)
                return BadRequest("Fecha inválida");

            if (login.TipoMov != 1 && login.TipoMov != 0)
                return BadRequest("TipoMov inválido. Solo 1 (login) o 0 (logout)");
                
            // Obtener el último movimiento del usuario para validar la secuencia
            var last = await _context.Logins
                .Where(l => l.User_id == login.User_id)
                .OrderByDescending(l => l.Fecha)
                .FirstOrDefaultAsync();

            // No permitir login si el último movimiento ya fue un login
            if (login.TipoMov == 1 && last != null && last.TipoMov == 1)
                return BadRequest("No puedes hacer login sin logout previo");

            // No permitir logout si no hay login previo
            if (login.TipoMov == 0 && (last == null || last.TipoMov == 0))
                return BadRequest("No puedes hacer logout sin login previo");

            _context.Logins.Add(login);
            await _context.SaveChangesAsync();

            return Ok(login);
        }

        // PUT /logins/{id}
        // Actualiza un registro existente
        [HttpPut("{user_id}/{fecha}")] //uso 2 claves porque uso clave compuesta
        public async Task<IActionResult> Update(int user_id, DateTime fecha, Login updated)
        {
            var login = await _context.Logins.FindAsync(user_id, fecha);

            if (login == null)
                return NotFound();

            login.Extension = updated.Extension;
            login.TipoMov = updated.TipoMov;
            login.Fecha = updated.Fecha;

            await _context.SaveChangesAsync();

            return Ok(login);
        }

        // DELETE /logins/{id}
        // Elimina un registro de la base de datos
        [HttpDelete("{user_id}/{fecha}")]
        public async Task<IActionResult> Delete(int user_id, DateTime fecha)
        {
            var login = await _context.Logins.FindAsync(user_id, fecha);

            //validar que el registro exista
            if (login == null)
                return NotFound();

            _context.Logins.Remove(login);
            await _context.SaveChangesAsync();

            return Ok("Eliminado");
        }
    }
}