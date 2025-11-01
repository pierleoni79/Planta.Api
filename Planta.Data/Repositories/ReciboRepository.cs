// Ruta: /Planta.Data/Repositories/ReciboRepository.cs | V1.0
#nullable enable
namespace Planta.Data.Repositories;

public sealed class ReciboRepository : IReciboRepository
{
    private readonly PlantaDbContext _db;
    public ReciboRepository(PlantaDbContext db) => _db = db;

    public Task<Recibo?> GetAsync(Guid id, CancellationToken ct = default)
        => _db.Recibos.Include("_historial").FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Recibo entity, CancellationToken ct = default)
    {
        await _db.Recibos.AddAsync(entity, ct);
    }
}
