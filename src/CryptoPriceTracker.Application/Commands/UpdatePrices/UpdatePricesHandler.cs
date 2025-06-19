using MediatR;
using CryptoPriceTracker.Application.Interfaces;
using CryptoPriceTracker.Domain.Entities;

namespace CryptoPriceTracker.Application.Commands.UpdatePrices;

public class UpdatePricesHandler :
    IRequestHandler<UpdatePricesCommand, UpdatePricesResult>
{
    private readonly ICryptoRepository _repo;
    private readonly IPriceFetcher _fetcher;

    public UpdatePricesHandler(ICryptoRepository repo, IPriceFetcher fetcher)
    {
        _repo    = repo;
        _fetcher = fetcher;
    }

    public async Task<UpdatePricesResult> Handle(
        UpdatePricesCommand request, CancellationToken ct)
    {
        try
        {
            // Get assets with their PriceHistory included
            var assets = await _repo.GetAssetsAsync();

            // Get prices from CoinGecko
            var pricesDict = await _fetcher.FetchPricesAsync(assets, ct);

            var toInsert = new List<CryptoPriceHistory>();

            foreach (var asset in assets)
            {
                if (!pricesDict.TryGetValue(asset.ExternalId, out var data))
                    continue; // Coin not returned

                var (price, tsUtc, icon) = data;

                // Update icon if empty
                if (string.IsNullOrEmpty(asset.IconUrl) && !string.IsNullOrEmpty(icon))
                    asset.IconUrl = icon;

                // Avoid duplicates using the loaded history
                bool exists = asset.PriceHistory?.Any(p => p.Date == tsUtc) ?? false;

                if (exists) continue;

                toInsert.Add(new CryptoPriceHistory
                {
                    CryptoAssetId = asset.Id,
                    Date          = tsUtc,
                    Price         = price
                });
            }

            // Save only new records
            await _repo.AddPricesAsync(toInsert, ct);

            return new UpdatePricesResult(true, toInsert.Count);
        }
        catch (Exception ex)
        {
            return new UpdatePricesResult(false, 0, ex.Message);
        }
    }
}

