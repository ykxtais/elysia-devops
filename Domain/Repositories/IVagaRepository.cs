namespace ElysiaAPI.Domain.Repositories;
using ElysiaAPI.Domain.Entity;

public interface IVagaRepository
{
    Task<Vaga?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Vaga vaga, CancellationToken ct = default);
    Task<bool> ExistsPatioNumeroAsync(string patio, int numero, CancellationToken ct = default);
}
