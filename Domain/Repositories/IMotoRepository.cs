namespace ElysiaAPI.Domain.Repositories;
using ElysiaAPI.Domain.Entity;
using ElysiaAPI.Domain.ValueObjects;

public interface IMotoRepository
{
    Task<Moto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Moto moto, CancellationToken ct = default);
    Task<bool> PlacaExistsAsync(Placa placa, CancellationToken ct = default);
}