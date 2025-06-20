using MediatR;
using CryptoPriceTracker.Application.Interfaces;
using CryptoPriceTracker.Domain.Entities;
using System.Linq;

namespace CryptoPriceTracker.Application.Queries.GetLatestPrices;

public class GetLatestPricesHandler :
    IRequestHandler<GetLatestPricesQuery, IReadOnlyList<LatestPriceDto>>
{
    private readonly ICryptoRepository _repo;

    public GetLatestPricesHandler(ICryptoRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<LatestPriceDto>> Handle(
        GetLatestPricesQuery request, CancellationToken ct)
    {
        var assets = await _repo.GetAssetsAsync();

        var list = assets.Select(a =>
        {
            // get latest 2 prices for this asset
            // if no history, return empty list
            var prices = a.PriceHistory != null
                ? a.PriceHistory.OrderByDescending(p => p.Date).Take(2).ToList()
                : new List<CryptoPriceHistory>();

            // Get latest and previous prices
            var latest  = prices.FirstOrDefault();  

            if (latest is null)
            {
                // Without history => return price 0 and neutral trend
                return new LatestPriceDto(
                    a.Name,
                    a.Symbol,
                    0m,
                    "USD",
                    a.IconUrl ?? string.Empty,
                    DateTime.UtcNow,
                    "➖",
                    null);
            }

            var previous = prices.Skip(1).FirstOrDefault(); 

            decimal latestPrice   = latest!.Price;          
            decimal previousPrice = previous?.Price ?? latestPrice;

            // Calculate trend
            string trend = latestPrice > previousPrice ? "🔼"
                        : latestPrice < previousPrice ? "🔽"
                        : "➖";
                        
            decimal? percentageChange = previousPrice == 0 ? null
            : Math.Round(((latestPrice - previousPrice) / previousPrice) * 100, 2);

            return new LatestPriceDto(
                a.Name,
                a.Symbol,
                latestPrice,
                "USD",
                a.IconUrl ?? string.Empty,
                latest?.Date ?? DateTime.UtcNow,
                trend,
                percentageChange
            );
        }).ToList();

        return list;
    }
}
