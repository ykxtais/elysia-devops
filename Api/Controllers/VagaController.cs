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
        
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ActionResult<object>> GetVagas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Vagas.AsNoTracking().OrderBy(v => v.Id);

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => ToResponse(v))
                .ToListAsync(ct);

            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

            return Ok(new
            {
                page,
                pageSize,
                total,
                totalPages,
                items
            });
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VagaResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VagaResponse>> GetVaga(int id)
        {
            var vaga = await _context.Vagas.FindAsync(id);
            if (vaga == null) return NotFound();
            return Ok(ToResponse(vaga));
        }
        
        [HttpGet("patio")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ActionResult<object>> GetVagasByPatio(
            [FromQuery] string patio,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var p = (patio ?? string.Empty).Trim();

            var query = _context.Vagas.AsNoTracking()
                .Where(v => v.Patio == p)
                .OrderBy(v => v.Id);

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => ToResponse(v))
                .ToListAsync(ct);

            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

            return Ok(new
            {
                page,
                pageSize,
                total,
                totalPages,
                items
            });
        }

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

                return CreatedAtAction(nameof(GetVaga), new { id = vaga.Id }, ToResponse(vaga));
            }
            catch (ArgumentException ex)           { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex)   { return BadRequest(ex.Message); }
            catch (DbUpdateException ex) when (
                (ex.InnerException?.Message?.Contains("ORA-00001") ?? false) ||
                (ex.InnerException?.Message?.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ?? false)
            )
            {
                return Conflict($"Já existe a vaga nº {request.Numero} no pátio '{request.Patio}'.");
            }
        }

        [HttpPut("{id:int}")]
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
            catch (ArgumentException ex)           { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex)   { return BadRequest(ex.Message); }
            catch (DbUpdateException ex) when (
                (ex.InnerException?.Message?.Contains("ORA-00001") ?? false) ||
                (ex.InnerException?.Message?.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ?? false)
            )
            {
                return Conflict($"Já existe a vaga nº {request.Numero} no pátio '{request.Patio}'.");
            }
        }

        [HttpDelete("{id:int}")]
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
