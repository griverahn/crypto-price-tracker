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
            var prices = a.PriceHistory
                          .OrderByDescending(p => p.Date)
                          .Take(2) // get latest and previous price
                          .ToList();

            // Get latest and previous prices
            var latest  = prices.FirstOrDefault();  
            var previous = prices.Skip(1).FirstOrDefault(); 

            decimal latestPrice   = latest!.Price;          
            decimal previousPrice = previous?.Price ?? latestPrice;

            // Calculate trend
            string trend = latestPrice > previousPrice ? "🔼"
                        : latestPrice < previousPrice ? "🔽"
                        : "➖";

            return new LatestPriceDto(
                a.Name,
                a.Symbol,
                latestPrice,
                "USD",
                a.IconUrl ?? string.Empty,
                latest?.Date ?? DateTime.UtcNow,
                trend
            );
        }).ToList();

        return list;
    }
}
