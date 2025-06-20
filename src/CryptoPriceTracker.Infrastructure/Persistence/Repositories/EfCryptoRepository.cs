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

    public async Task UpdateAssetAsync(CryptoAsset asset, CancellationToken ct)
    {
        _db.CryptoAssets.Update(asset);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddPricesAsync(IEnumerable<CryptoPriceHistory> prices, CancellationToken ct = default)
    {
        _db.CryptoPrices.AddRange(prices);
        await _db.SaveChangesAsync(ct);
    }

    // Fetch price history for a specific crypto asset symbol since a given date
    public Task<IReadOnlyList<CryptoPriceHistory>> GetPriceHistoryAsync(
        string symbol, DateTime sinceUtc, CancellationToken ct = default)
    {
        return _db.CryptoPrices
            .Where(p => p.CryptoAsset != null &&
                        p.CryptoAsset.Symbol == symbol &&
                        p.Date >= sinceUtc)
            .OrderBy(p => p.Date)
            .AsNoTracking()
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<CryptoPriceHistory>>(t => t.Result, ct);
    }
}
