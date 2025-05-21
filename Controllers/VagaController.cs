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

        public VagaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VagaResponse>), 200)]
        public async Task<ActionResult<IEnumerable<VagaResponse>>> GetVagas()
        {
            var vagas = await _context.Vagas.ToListAsync();
            var responses = vagas.Select(v => new VagaResponse
            {
                Id = v.Id,
                Status = v.Status,
                Numero = v.Numero,
                Patio = v.Patio
            });

            return Ok(responses);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VagaResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VagaResponse>> GetVaga(int id)
        {
            var vaga = await _context.Vagas.FindAsync(id);

            if (vaga == null)
                return NotFound();

            var response = new VagaResponse
            {
                Id = vaga.Id,
                Status = vaga.Status,
                Numero = vaga.Numero,
                Patio = vaga.Patio
            };

            return Ok(response);
        }

        [HttpGet("patio")]
        [ProducesResponseType(typeof(IEnumerable<VagaResponse>), 200)]
        public async Task<ActionResult<IEnumerable<VagaResponse>>> GetVagasByPatio([FromQuery] string patio)
        {
            var vagas = await _context.Vagas
                .Where(v => v.Patio == patio)
                .ToListAsync();

            var responses = vagas.Select(v => new VagaResponse
            {
                Id = v.Id,
                Status = v.Status,
                Numero = v.Numero,
                Patio = v.Patio
            });

            return Ok(responses);
        }

        [HttpPost]
        [ProducesResponseType(typeof(VagaResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<VagaResponse>> CreateVaga(VagaRequest request)
        {
            var vaga = new Vaga
            {
                Status = request.Status,
                Numero = request.Numero,
                Patio = request.Patio
            };

            _context.Vagas.Add(vaga);
            await _context.SaveChangesAsync();

            var response = new VagaResponse
            {
                Id = vaga.Id,
                Status = vaga.Status,
                Numero = vaga.Numero,
                Patio = vaga.Patio
            };

            return CreatedAtAction(nameof(GetVaga), new { id = vaga.Id }, response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateVaga(int id, VagaRequest request)
        {
            var vaga = await _context.Vagas.FindAsync(id);

            if (vaga == null)
                return NotFound();

            vaga.Status = request.Status;
            vaga.Numero = request.Numero;
            vaga.Patio = request.Patio;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteVaga(int id)
        {
            var vaga = await _context.Vagas.FindAsync(id);

            if (vaga == null)
                return NotFound();

            _context.Vagas.Remove(vaga);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
