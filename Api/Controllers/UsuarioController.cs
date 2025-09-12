using ElysiaAPI.Application.DTOs.Request;
using ElysiaAPI.Application.DTOs.Response;
using ElysiaAPI.Application.DTOs.Hateoas;
using ElysiaAPI.Domain.Entity;
using ElysiaAPI.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ElysiaAPI.Controllers
{
    /// <summary>Endpoints para gerenciamento de usuários.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsuarioController(AppDbContext context) => _context = context;

        private static UsuarioResponse ToResponse(Usuario u) => new()
        {
            Id    = u.Id,
            Nome  = u.Nome,
            Email = u.Email,
            Cpf   = u.Cpf
        };

        private UsuarioResponse WithLinks(UsuarioResponse r)
        {
            r.Links.Clear();

            var self   = Url.Link(nameof(GetUsuario),    new { id = r.Id });
            var update = Url.Link(nameof(UpdateUsuario), new { id = r.Id });
            var delete = Url.Link(nameof(DeleteUsuario), new { id = r.Id });

            if (self   is not null) r.Links.Add(new Link("self",   self,   "GET"));
            if (update is not null) r.Links.Add(new Link("update", update, "PUT"));
            if (delete is not null) r.Links.Add(new Link("delete", delete, "DELETE"));

            return r;
        }

        /// <summary>Busca um usuário por ID.</summary>
        [SwaggerOperation(Summary = "Busca um usuário por ID")]
        [HttpGet("{id:int}", Name = nameof(GetUsuario))]
        [ProducesResponseType(typeof(UsuarioResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UsuarioResponse>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return Ok(WithLinks(ToResponse(usuario)));
        }

        /// <summary>Cadastra um novo usuário.</summary>
        [SwaggerOperation(Summary = "Cadastra um novo usuário")]
        [HttpPost]
        [ProducesResponseType(typeof(UsuarioResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UsuarioResponse>> CreateUsuario([FromBody] UsuarioRequest request)
        {
            try
            {
                var usuario = new Usuario(request.Nome, request.Email, request.Senha, request.Cpf);
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var resp = WithLinks(ToResponse(usuario));
                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, resp);
            }
            catch (ArgumentException ex)         { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        /// <summary>Atualiza um usuário existente.</summary>
        [SwaggerOperation(Summary = "Atualiza um usuário existente")]
        [HttpPut("{id:int}", Name = nameof(UpdateUsuario))]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUsuario(int id, [FromBody] UsuarioRequest request)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
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

        /// <summary>Remove um usuário por ID.</summary>
        [SwaggerOperation(Summary = "Remove um usuário")]
        [HttpDelete("{id:int}", Name = nameof(DeleteUsuario))]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
