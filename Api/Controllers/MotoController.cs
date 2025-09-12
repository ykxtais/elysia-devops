using ElysiaAPI.Application.DTOs.Request;
using ElysiaAPI.Application.DTOs.Response;
using ElysiaAPI.Application.DTOs.Hateoas;
using ElysiaAPI.Domain.Entity;
using ElysiaAPI.Domain.ValueObjects;
using ElysiaAPI.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElysiaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MotoController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MotoController(AppDbContext context) => _context = context;

        private static MotoResponse ToResponse(Moto m) => new()
        {
            Id = m.Id,
            Placa = m.Placa.Value,
            Marca = m.Marca,
            Modelo = m.Modelo,
            Ano = m.Ano
        };
        
        private MotoResponse WithLinks(MotoResponse r)
        {
            r.Links.Clear();
            r.Links.Add(new Link("self",   Url.Link(nameof(GetMoto),    new { id = r.Id })!));
            r.Links.Add(new Link("update", Url.Link(nameof(UpdateMoto), new { id = r.Id })!, "PUT"));
            r.Links.Add(new Link("delete", Url.Link(nameof(DeleteMoto), new { id = r.Id })!, "DELETE"));
            r.Links.Add(new Link("list",   Url.Link(nameof(GetMotos),   new { page = 1, pageSize = 10 })!));
            return r;
        }

        private List<Link> CollectionLinks(string routeName, int page, int pageSize, int total, object? extra = null)
        {
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
            object Params(int p) => extra is null ? new { page = p, pageSize } : extra;
            var links = new List<Link> { new("self", Url.Link(routeName, Params(page))!) };
            if (page > 1)          links.Add(new("prev", Url.Link(routeName, Params(page - 1))!));
            if (page < totalPages) links.Add(new("next", Url.Link(routeName, Params(page + 1))!));
            return links;
        }

        [HttpGet(Name = nameof(GetMotos))]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ActionResult<object>> GetMotos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Motos.AsNoTracking().OrderBy(m => m.Id);

            var total = await query.CountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

            return Ok(new
            {
                page,
                pageSize,
                total,
                totalPages,
                items = items.Select(m => WithLinks(ToResponse(m))),
                _links = CollectionLinks(nameof(GetMotos), page, pageSize, total)
            });
        }

        [HttpGet("{id:int}", Name = nameof(GetMoto))]
        [ProducesResponseType(typeof(MotoResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MotoResponse>> GetMoto(int id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null) return NotFound();
            return Ok(WithLinks(ToResponse(moto)));
        }
        
        [HttpGet("search", Name = nameof(SearchMoto))]
        [ProducesResponseType(typeof(IEnumerable<MotoResponse>), 200)]
        public async Task<ActionResult<IEnumerable<MotoResponse>>> SearchMoto([FromQuery] string placa)
        {
            placa ??= string.Empty;
            var motos = await _context.Motos
                .AsNoTracking()
                .Where(m => EF.Functions.Like(m.Placa.Value, $"%{placa}%"))
                .OrderBy(m => m.Id)
                .ToListAsync();

            return Ok(motos.Select(m => WithLinks(ToResponse(m))));
        }

        [HttpPost]
        [ProducesResponseType(typeof(MotoResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MotoResponse>> CreateMoto([FromBody] MotoRequest request)
        {
            try
            {
                var moto = new Moto(
                    Placa.Create(request.Placa),
                    request.Marca,
                    request.Modelo,
                    request.Ano
                );

                _context.Motos.Add(moto);
                await _context.SaveChangesAsync();

                var resp = WithLinks(ToResponse(moto));
                return CreatedAtAction(nameof(GetMoto), new { id = moto.Id }, resp);
            }
            catch (ArgumentException ex)           { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex)   { return BadRequest(ex.Message); }
        }

        [HttpPut("{id:int}", Name = nameof(UpdateMoto))]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateMoto(int id, [FromBody] MotoRequest request)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null) return NotFound();

            try
            {
                moto.DefinirPlaca(Placa.Create(request.Placa));
                moto.AtualizarDadosBasicos(request.Marca, request.Modelo, request.Ano);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (ArgumentException ex)           { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex)   { return BadRequest(ex.Message); }
        }

        [HttpDelete("{id:int}", Name = nameof(DeleteMoto))]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMoto(int id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null) return NotFound();

            _context.Motos.Remove(moto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
