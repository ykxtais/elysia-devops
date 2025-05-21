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
    public class MotoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MotoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MotoResponse>), 200)]
        public async Task<ActionResult<IEnumerable<MotoResponse>>> GetMotos()
        {
            var motos = await _context.Motos.ToListAsync();
            var responses = motos.Select(m => new MotoResponse
            {
                Id = m.Id,
                Placa = m.Placa,
                Marca = m.Marca,
                Modelo = m.Modelo,
                Ano = m.Ano
            });

            return Ok(responses);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MotoResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MotoResponse>> GetMoto(int id)
        {
            var moto = await _context.Motos.FindAsync(id);

            if (moto == null)
                return NotFound();

            var response = new MotoResponse
            {
                Id = moto.Id,
                Placa = moto.Placa,
                Marca = moto.Marca,
                Modelo = moto.Modelo,
                Ano = moto.Ano
            };

            return Ok(response);
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<MotoResponse>), 200)]
        public async Task<ActionResult<IEnumerable<MotoResponse>>> SearchMoto([FromQuery] string placa)
        {
            var motos = await _context.Motos
                .Where(m => m.Placa.Contains(placa))
                .ToListAsync();

            var responses = motos.Select(m => new MotoResponse
            {
                Id = m.Id,
                Placa = m.Placa,
                Marca = m.Marca,
                Modelo = m.Modelo,
                Ano = m.Ano
            });

            return Ok(responses);
        }

        [HttpPost]
        [ProducesResponseType(typeof(MotoResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MotoResponse>> CreateMoto(MotoRequest request)
        {
            var moto = new Moto
            {
                Placa = request.Placa,
                Marca = request.Marca,
                Modelo = request.Modelo,
                Ano = request.Ano
            };

            _context.Motos.Add(moto);
            await _context.SaveChangesAsync();

            var response = new MotoResponse
            {
                Id = moto.Id,
                Placa = moto.Placa,
                Marca = moto.Marca,
                Modelo = moto.Modelo,
                Ano = moto.Ano
            };

            return CreatedAtAction(nameof(GetMoto), new { id = moto.Id }, response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateMoto(int id, MotoRequest request)
        {
            var moto = await _context.Motos.FindAsync(id);

            if (moto == null)
                return NotFound();

            moto.Placa = request.Placa;
            moto.Marca = request.Marca;
            moto.Modelo = request.Modelo;
            moto.Ano = request.Ano;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMoto(int id)
        {
            var moto = await _context.Motos.FindAsync(id);

            if (moto == null)
                return NotFound();

            _context.Motos.Remove(moto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
