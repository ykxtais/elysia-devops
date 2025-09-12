using ElysiaAPI.Application.DTOs.Request;
using ElysiaAPI.Application.DTOs.Response;
using ElysiaAPI.Application.DTOs.Hateoas;
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
        
        private UsuarioResponse WithLinks(UsuarioResponse r)
        {
            r.Links.Clear();
            r.Links.Add(new Link("self",   Url.Link(nameof(GetUsuario),    new { id = r.Id })!));
            r.Links.Add(new Link("update", Url.Link(nameof(UpdateUsuario), new { id = r.Id })!, "PUT"));
            r.Links.Add(new Link("delete", Url.Link(nameof(DeleteUsuario), new { id = r.Id })!, "DELETE"));
            r.Links.Add(new Link("list",   Url.Link(nameof(GetUsuarios),   new { page = 1, pageSize = 10 })!));
            return r;
        }

        private List<Link> CollectionLinks(string routeName, int page, int pageSize, int total)
        {
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
            object Params(int p) => new { page = p, pageSize };
            var links = new List<Link> { new("self", Url.Link(routeName, Params(page))!) };
            if (page > 1)          links.Add(new("prev", Url.Link(routeName, Params(page - 1))!));
            if (page < totalPages) links.Add(new("next", Url.Link(routeName, Params(page + 1))!));
            return links;
        }

        [HttpGet(Name = nameof(GetUsuarios))]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ActionResult<object>> GetUsuarios(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Usuarios.AsNoTracking().OrderBy(u => u.Id);

            var total = await query.CountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

            return Ok(new
            {
                page,
                pageSize,
                total,
                totalPages,
                items = items.Select(u => WithLinks(ToResponse(u))),
                _links = CollectionLinks(nameof(GetUsuarios), page, pageSize, total)
            });
        }

        [HttpGet("{id:int}", Name = nameof(GetUsuario))]
        [ProducesResponseType(typeof(UsuarioResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UsuarioResponse>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return Ok(WithLinks(ToResponse(usuario)));
        }

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
