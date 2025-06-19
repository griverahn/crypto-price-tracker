namespace CryptoPriceTracker.Application.Interfaces;
using CryptoPriceTracker.Domain.Entities;

public interface IPriceFetcher
{
    Task<IReadOnlyDictionary<string,
        (decimal price, DateTime timestamp, string iconUrl)>> FetchPricesAsync(
            IEnumerable<CryptoAsset> assets, CancellationToken ct = default);
}
