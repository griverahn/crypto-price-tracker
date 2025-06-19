namespace CryptoPriceTracker.Application.Interfaces;
using CryptoPriceTracker.Domain.Entities;

public interface ICryptoRepository
{
    Task<IReadOnlyList<CryptoAsset>> GetAssetsAsync();
    Task AddPricesAsync(IEnumerable<CryptoPriceHistory> prices, CancellationToken ct = default);
}
