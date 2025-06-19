using CryptoPriceTracker.Domain.Entities;
using CryptoPriceTracker.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CryptoPriceTracker.Infrastructure.Persistence.Repositories;

public class EfCryptoRepository : ICryptoRepository
{
    private readonly AppDbContext _db;

    public EfCryptoRepository(AppDbContext db) => _db = db;

    public Task<IReadOnlyList<CryptoAsset>> GetAssetsAsync() =>
        _db.CryptoAssets
            .Include(a => a.PriceHistory) // Include price history for each asset
            .AsNoTracking()
            .ToListAsync()
            .ContinueWith<IReadOnlyList<CryptoAsset>>(t => t.Result);

    public async Task AddPricesAsync(IEnumerable<CryptoPriceHistory> prices, CancellationToken ct = default)
    {
        _db.CryptoPrices.AddRange(prices);
        await _db.SaveChangesAsync(ct);
    }
}
