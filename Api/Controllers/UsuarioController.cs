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
            var list   = Url.Link(nameof(GetUsuarios),   new { page = 1, pageSize = 10 });

            if (self   is not null) r.Links.Add(new Link("self",   self));             
            if (update is not null) r.Links.Add(new Link("update", update, "PUT"));   
            if (delete is not null) r.Links.Add(new Link("delete", delete, "DELETE")); 
            if (list   is not null) r.Links.Add(new Link("list",   list, "GET"));      

            return r;
        }

        private static int Clamp(int value, int min, int max)
            => value < min ? min : (value > max ? max : value);

        private List<Link> CollectionLinks(string routeName, int page, int pageSize, int total)
        {
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
            object Params(int p) => new { page = p, pageSize };

            var links = new List<Link> { new("self", Url.Link(routeName, Params(page))!) };
            if (page > 1)          links.Add(new("prev", Url.Link(routeName, Params(page - 1))!));
            if (page < totalPages) links.Add(new("next", Url.Link(routeName, Params(page + 1))!));
            return links;
        }

        /// <summary>
        /// Retorna usuários paginados.
        /// </summary>
        /// <param name="page">Página atual (mínimo 1).</param>
        /// <param name="pageSize">Tamanho da página (1 a 100).</param>
        /// <param name="ct">CancellationToken.</param>
        /// <returns>Paginação, links HATEOAS e itens.</returns>
        /// <response code="200">Lista paginada retornada com sucesso.</response>
        [HttpGet(Name = nameof(GetUsuarios))]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ActionResult<object>> GetUsuarios(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            page     = Clamp(page, 1, int.MaxValue);
            pageSize = Clamp(pageSize, 1, 100);

            var query = _context.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.Id);

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

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

        /// <summary>
        /// Retorna um usuário pelo <paramref name="id"/>.
        /// </summary>
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

        /// <summary>
        /// Cria um novo usuário.
        /// </summary>
        /// <param name="request">Dados de criação (Nome, Email, Senha, Cpf).</param>
        /// <returns>Usuário criado com links HATEOAS.</returns>
        /// <response code="201">Usuário criado com sucesso.</response>
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

        /// <summary>
        /// Atualiza um usuário existente.
        /// </summary>
        /// <param name="id">Id do usuário.</param>
        /// <param name="request">Dados de atualização (Nome, Email, Senha, Cpf).</param>
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

        /// <summary>
        /// Remove um usuário.
        /// </summary>
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
