using ElysiaAPI.Application.DTOs.Request;
using ElysiaAPI.Application.DTOs.Response;
using ElysiaAPI.Application.DTOs.Hateoas;
using ElysiaAPI.Domain.Entity;
using ElysiaAPI.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;

namespace ElysiaAPI.Controllers
{
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

            if (self   is not null)   r.Links.Add(new Link("self",   self));               // GET
            if (update is not null)   r.Links.Add(new Link("update", update, "PUT"));      // PUT
            if (delete is not null)   r.Links.Add(new Link("delete", delete, "DELETE"));   // DELETE

            return r;
        }

        /// <summary>Busca um usuário por ID.</summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <returns>Usuário com links HATEOAS.</returns>
        /// <response code="200">Usuário encontrado.</response>
        /// <response code="404">Usuário não encontrado.</response>
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
        /// <param name="request">Nome, Email, Senha, Cpf.</param>
        /// <returns>Usuário criado com links HATEOAS.</returns>
        /// <response code="201">Criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
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
        /// <param name="id">Id do usuário.</param>
        /// <param name="request">Nome, Email, Senha, Cpf.</param>
        /// <response code="204">Atualizado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="404">Usuário não encontrado.</response>
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

        /// <summary>Remove um usuário.</summary>
        /// <param name="id">Id do usuário.</param>
        /// <response code="204">Removido com sucesso.</response>
        /// <response code="404">Usuário não encontrado.</response>
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
