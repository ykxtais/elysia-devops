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
    public class VagaController : ControllerBase
    {
        private readonly AppDbContext _context;
        public VagaController(AppDbContext context) => _context = context;

        private static VagaResponse ToResponse(Vaga v) => new()
        {
            Id = v.Id,
            Status = v.Status,
            Numero = v.Numero,
            Patio = v.Patio
        };

        private VagaResponse WithLinks(VagaResponse r)
        {
            r.Links.Clear();
            r.Links.Add(new Link("self",   Url.Link(nameof(GetVaga),    new { id = r.Id })!));
            r.Links.Add(new Link("update", Url.Link(nameof(UpdateVaga), new { id = r.Id })!, "PUT"));
            r.Links.Add(new Link("delete", Url.Link(nameof(DeleteVaga), new { id = r.Id })!, "DELETE"));

            if (string.Equals(r.Status, "Livre", StringComparison.OrdinalIgnoreCase))
                r.Links.Add(new Link("ocupar", Url.Link(nameof(UpdateVaga), new { id = r.Id })!, "PUT"));
            if (string.Equals(r.Status, "Ocupada", StringComparison.OrdinalIgnoreCase))
                r.Links.Add(new Link("liberar", Url.Link(nameof(UpdateVaga), new { id = r.Id })!, "PUT"));

            return r;
        }

        private List<Link> CollectionLinks(string routeName, int page, int pageSize, int total, string? patio = null)
        {
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

            object Params(int p) => patio is null
                ? new { page = p, pageSize }
                : new { patio, page = p, pageSize };

            var links = new List<Link> { new("self", Url.Link(routeName, Params(page))!) };
            if (page > 1)          links.Add(new("prev", Url.Link(routeName, Params(page - 1))!));
            if (page < totalPages) links.Add(new("next", Url.Link(routeName, Params(page + 1))!));
            return links;
        }

        /// <summary>Lista paginada de vagas.</summary>
        /// <param name="page">Página.</param>
        /// <param name="pageSize">Itens por página (1..100).</param>
        /// <returns>Paginação, itens e links HATEOAS.</returns>
        [HttpGet(Name = nameof(GetVagas))]
        [ProducesResponseType(typeof(object), 200)]
        public ActionResult<object> GetVagas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Vagas
                .AsNoTracking()
                .OrderBy(v => v.Id);

            var total = query.Count();
            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => ToResponse(v))
                .ToList();

            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

            return Ok(new
            {
                page,
                pageSize,
                total,
                totalPages,
                items = items.Select(WithLinks),
                _links = CollectionLinks(nameof(GetVagas), page, pageSize, total)
            });
        }

        /// <summary>Obtém uma vaga por ID.</summary>
        /// <param name="id">Identificador da vaga.</param>
        [HttpGet("{id:int}", Name = nameof(GetVaga))]
        [ProducesResponseType(typeof(VagaResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VagaResponse>> GetVaga(int id)
        {
            var vaga = await _context.Vagas.FindAsync(id);
            if (vaga == null) return NotFound();
            return Ok(WithLinks(ToResponse(vaga)));
        }

        /// <summary>Lista paginada de vagas filtrando por pátio.</summary>
        /// <param name="patio">Nome do pátio.</param>
        /// <param name="page">Página.</param>
        /// <param name="pageSize">Itens por página (1..100).</param>
        [HttpGet("patio", Name = nameof(GetVagasByPatio))]
        [ProducesResponseType(typeof(object), 200)]
        public ActionResult<object> GetVagasByPatio(
            [FromQuery] string patio,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var p = (patio ?? string.Empty).Trim();

            var query = _context.Vagas
                .AsNoTracking()
                .Where(v => v.Patio == p)
                .OrderBy(v => v.Id);

            var total = query.Count();
            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => ToResponse(v))
                .ToList();

            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

            return Ok(new
            {
                page,
                pageSize,
                total,
                totalPages,
                items = items.Select(WithLinks),
                _links = CollectionLinks(nameof(GetVagasByPatio), page, pageSize, total, p)
            });
        }

        /// <summary>Cria uma nova vaga.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(VagaResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<VagaResponse>> CreateVaga([FromBody] VagaRequest request)
        {
            try
            {
                var vaga = new Vaga(request.Numero, request.Patio);
                if (!string.IsNullOrWhiteSpace(request.Status))
                    vaga.Status = request.Status;

                _context.Vagas.Add(vaga);
                await _context.SaveChangesAsync();

                var resp = WithLinks(ToResponse(vaga));
                return CreatedAtAction(nameof(GetVaga), new { id = vaga.Id }, resp);
            }
            catch (ArgumentException ex)         { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (DbUpdateException ex) when (
                (ex.InnerException?.Message?.Contains("ORA-00001") ?? false) ||
                (ex.InnerException?.Message?.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                return Conflict($"Já existe a vaga nº {request.Numero} no pátio '{request.Patio}'.");
            }
        }

        /// <summary>Atualiza dados de uma vaga.</summary>
        [HttpPut("{id:int}", Name = nameof(UpdateVaga))]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> UpdateVaga(int id, [FromBody] VagaRequest request)
        {
            var vaga = await _context.Vagas.FindAsync(id);
            if (vaga == null) return NotFound();

            try
            {
                vaga.AtualizarLocalizacao(request.Numero, request.Patio);
                vaga.Status = request.Status;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (ArgumentException ex)         { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (DbUpdateException ex) when (
                (ex.InnerException?.Message?.Contains("ORA-00001") ?? false) ||
                (ex.InnerException?.Message?.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                return Conflict($"Já existe a vaga nº {request.Numero} no pátio '{request.Patio}'.");
            }
        }

        /// <summary>Remove uma vaga.</summary>
        [HttpDelete("{id:int}", Name = nameof(DeleteVaga))]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteVaga(int id)
        {
            var vaga = await _context.Vagas.FindAsync(id);
            if (vaga == null) return NotFound();

            _context.Vagas.Remove(vaga);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
