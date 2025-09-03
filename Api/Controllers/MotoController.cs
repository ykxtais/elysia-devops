using ElysiaAPI.Application.DTOs.Request;
using ElysiaAPI.Application.DTOs.Response;
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

        // mapper DRY
        private static MotoResponse ToResponse(Moto m) => new()
        {
            Id = m.Id,
            Placa = m.Placa.Value,
            Marca = m.Marca,
            Modelo = m.Modelo,
            Ano = m.Ano
        };

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MotoResponse>), 200)]
        public async Task<ActionResult<IEnumerable<MotoResponse>>> GetMotos()
        {
            var motos = await _context.Motos.AsNoTracking().ToListAsync();
            return Ok(motos.Select(ToResponse));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(MotoResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MotoResponse>> GetMoto(int id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null) return NotFound();
            return Ok(ToResponse(moto));
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<MotoResponse>), 200)]
        public async Task<ActionResult<IEnumerable<MotoResponse>>> SearchMoto([FromQuery] string placa)
        {
            placa ??= string.Empty;

            var motos = await _context.Motos
                .AsNoTracking()
                .Where(m => EF.Functions.Like(m.Placa.Value, $"%{placa}%"))
                .ToListAsync();

            return Ok(motos.Select(ToResponse));
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

                return CreatedAtAction(nameof(GetMoto), new { id = moto.Id }, ToResponse(moto));
            }
            catch (ArgumentException ex)           { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex)   { return BadRequest(ex.Message); }
        }

        [HttpPut("{id:int}")]
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

        [HttpDelete("{id:int}")]
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
