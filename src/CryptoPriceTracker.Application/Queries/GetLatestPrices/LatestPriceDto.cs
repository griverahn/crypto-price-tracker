namespace CryptoPriceTracker.Application.Queries.GetLatestPrices;

public record LatestPriceDto(
    string Name,
    string Symbol,
    decimal Price,
    string Currency,
    string IconUrl,
    DateTime TimestampUtc,
    string Trend,
    decimal? PercentageChange); 