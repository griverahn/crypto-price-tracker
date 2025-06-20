namespace CryptoPriceTracker.Application.Interfaces;
using CryptoPriceTracker.Domain.Entities;

public interface ICryptoRepository
{
    Task<IReadOnlyList<CryptoAsset>> GetAssetsAsync();
    Task AddPricesAsync(IEnumerable<CryptoPriceHistory> prices, CancellationToken ct = default);
    Task UpdateAssetAsync(CryptoAsset asset, CancellationToken ct = default);
    Task<IReadOnlyList<CryptoPriceHistory>> GetPriceHistoryAsync(
        string symbol, DateTime sinceUtc, CancellationToken ct = default);
}
