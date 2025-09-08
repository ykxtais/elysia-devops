using ElysiaAPI.Application.DTOs.Request;
using ElysiaAPI.Application.DTOs.Response;
using ElysiaAPI.Domain.Entity;
using ElysiaAPI.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElysiaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsuarioController(AppDbContext context) => _context = context;
        
        private static UsuarioResponse ToResponse(Usuario u) => new()
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            Cpf = u.Cpf
        };

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponse>), 200)]
        public async Task<ActionResult<IEnumerable<UsuarioResponse>>> GetUsuarios()
        {
            var usuarios = await _context.Set<Usuario>().AsNoTracking().ToListAsync();
            return Ok(usuarios.Select(ToResponse));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UsuarioResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UsuarioResponse>> GetUsuario(int id)
        {
            var usuario = await _context.Set<Usuario>().FindAsync(id);
            if (usuario == null) return NotFound();
            return Ok(ToResponse(usuario));
        }

        [HttpPost]
        [ProducesResponseType(typeof(UsuarioResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UsuarioResponse>> CreateUsuario([FromBody] UsuarioRequest request)
        {
            try
            {
                var usuario = new Usuario(request.Nome, request.Email, request.Senha, request.Cpf);

                _context.Set<Usuario>().Add(usuario);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, ToResponse(usuario));
            }
            catch (ArgumentException ex)         { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUsuario(int id, [FromBody] UsuarioRequest request)
        {
            var usuario = await _context.Set<Usuario>().FindAsync(id);
            if (usuario == null) return NotFound();

            try
            {
                usuario.AtualizarDadosBasicos(request.Nome, request.Email, request.Cpf);
                usuario.DefinirSenha(request.Senha);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (ArgumentException ex)         { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Set<Usuario>().FindAsync(id);
            if (usuario == null) return NotFound();

            _context.Set<Usuario>().Remove(usuario);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
