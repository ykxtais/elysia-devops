using ElysiaAPI.Application.DTOs.Request;
using ElysiaAPI.Application.DTOs.Response;
using ElysiaAPI.Application.DTOs.Hateoas;
using ElysiaAPI.Domain.Entity;
using ElysiaAPI.Domain.ValueObjects;
using ElysiaAPI.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace ElysiaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
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

        /// <summary>Lista motos paginadas.</summary>
        /// <remarks>
        /// Parâmetros: <c>page</c> (>=1), <c>pageSize</c> (1–100).  
        /// **Exemplo de resposta**:
        /// 
        /// ```json
        /// {
        ///   "page": 1,
        ///   "pageSize": 10,
        ///   "total": 2,
        ///   "totalPages": 1,
        ///   "items": [
        ///     { "id": 1, "placa": "KAC7516", "marca": "Honda", "modelo": "CG 160", "ano": 2021,
        ///       "links": [
        ///         { "rel": "self", "href": "/api/moto/1", "method": "GET" }
        ///       ]
        ///     }
        ///   ],
        ///   "_links": [
        ///     { "rel": "self", "href": "/api/moto?page=1&pageSize=10", "method": "GET" }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        [HttpGet(Name = nameof(GetMotos))]
        [SwaggerOperation(Summary = "Lista motos paginadas", Description = "Retorna coleção paginada com links HATEOAS.")]
        [SwaggerResponse(200, "Lista paginada retornada com sucesso")]
        public async Task<ActionResult<object>> GetMotos(
            [FromQuery, SwaggerParameter("Página (>= 1)", Required = false)] int page = 1,
            [FromQuery, SwaggerParameter("Tamanho da página (1–100)", Required = false)] int pageSize = 10,
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

        /// <summary>Busca uma moto por ID.</summary>
        /// <remarks>
        /// **Exemplo de resposta**:
        /// ```json
        /// { "id": 1, "placa": "KAC7516", "marca": "Honda", "modelo": "CG 160", "ano": 2021,
        ///   "links": [
        ///     { "rel": "self", "href": "/api/moto/1", "method": "GET" },
        ///     { "rel": "update", "href": "/api/moto/1", "method": "PUT" },
        ///     { "rel": "delete", "href": "/api/moto/1", "method": "DELETE" }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        [HttpGet("{id:int}", Name = nameof(GetMoto))]
        [SwaggerOperation(Summary = "Busca moto por ID", Description = "Retorna 404 se a moto não existir.")]
        [SwaggerResponse(200, "Moto encontrada", typeof(MotoResponse))]
        [SwaggerResponse(404, "Moto não encontrada")]
        public async Task<ActionResult<MotoResponse>> GetMoto(
            [FromRoute, SwaggerParameter("Identificador da moto", Required = true)] int id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null) return NotFound();
            return Ok(WithLinks(ToResponse(moto)));
        }

        /// <summary>Pesquisa motos pela placa (like).</summary>
        /// <remarks>
        /// **Exemplo**: <code>/api/moto/search?placa=KAC7516</code>  
        /// **Exemplo de resposta**:
        /// ```json
        /// [
        ///   { "id": 1, "placa": "KAC7516", "marca": "Honda", "modelo": "CG 160", "ano": 2021,
        ///     "links": [{ "rel": "self", "href": "/api/moto/1", "method": "GET" }]
        ///   }
        /// ]
        /// ```
        /// </remarks>
        [HttpGet("search", Name = nameof(SearchMoto))]
        [SwaggerOperation(Summary = "Pesquisa por placa", Description = "Busca parcial no valor da placa.")]
        [SwaggerResponse(200, "Lista de motos que correspondem ao filtro", typeof(IEnumerable<MotoResponse>))]
        public async Task<ActionResult<IEnumerable<MotoResponse>>> SearchMoto(
            [FromQuery, SwaggerParameter("Trecho da placa a ser pesquisado", Required = false)] string placa)
        {
            placa ??= string.Empty;
            var motos = await _context.Motos
                .AsNoTracking()
                .Where(m => EF.Functions.Like(m.Placa.Value, $"%{placa}%"))
                .OrderBy(m => m.Id)
                .ToListAsync();

            return Ok(motos.Select(m => WithLinks(ToResponse(m))));
        }

        /// <summary>Cria uma nova moto.</summary>
        /// <remarks>
        /// **Exemplo de request**:
        /// ```json
        /// { "placa": "KAC7516", "marca": "Honda", "modelo": "CG 160", "ano": 2021 }
        /// ```
        /// **Exemplo de response (201)**:
        /// ```json
        /// { "id": 1, "placa": "KAC7516", "marca": "Honda", "modelo": "CG 160", "ano": 2021,
        ///   "links": [{ "rel": "self", "href": "/api/moto/5", "method": "GET" }]
        /// }
        /// ```
        /// </remarks>
        [HttpPost]
        [SwaggerOperation(Summary = "Cria uma nova moto", Description = "Valida placa e dados básicos.")]
        [SwaggerResponse(201, "Moto criada com sucesso", typeof(MotoResponse))]
        [SwaggerResponse(400, "Dados inválidos")]
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
            catch (ArgumentException ex)         { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        /// <summary>Atualiza uma moto existente.</summary>
        /// <remarks>
        /// **Exemplo de request**:
        /// ```json
        /// { "placa": "KXH4827", "marca": "Yamaha", "modelo": "Fazer 250", "ano": 2023 }
        /// ```
        /// </remarks>
        [HttpPut("{id:int}", Name = nameof(UpdateMoto))]
        [SwaggerOperation(Summary = "Atualiza moto por ID", Description = "Retorna 404 se não existir.")]
        [SwaggerResponse(204, "Atualizado com sucesso")]
        [SwaggerResponse(400, "Dados inválidos")]
        [SwaggerResponse(404, "Moto não encontrada")]
        public async Task<IActionResult> UpdateMoto(
            [FromRoute, SwaggerParameter("Identificador da moto", Required = true)] int id,
            [FromBody] MotoRequest request)
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
            catch (ArgumentException ex)         { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        /// <summary>Remove uma moto por ID.</summary>
        [HttpDelete("{id:int}", Name = nameof(DeleteMoto))]
        [SwaggerOperation(Summary = "Remove moto por ID")]
        [SwaggerResponse(204, "Excluída com sucesso")]
        [SwaggerResponse(404, "Moto não encontrada")]
        public async Task<IActionResult> DeleteMoto(
            [FromRoute, SwaggerParameter("Identificador da moto", Required = true)] int id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null) return NotFound();

            _context.Motos.Remove(moto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
