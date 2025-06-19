using System.Text.Json;
using CryptoPriceTracker.Application.Interfaces;
using CryptoPriceTracker.Domain.Entities;

namespace CryptoPriceTracker.Infrastructure.ExternalApis;

public class CoinGeckoHttpClient : IPriceFetcher
{
    private readonly HttpClient _http;

    public CoinGeckoHttpClient(HttpClient http)
    {
        _http = http;

        // CoinGecko refuses requests without User-Agent
        if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
            _http.DefaultRequestHeaders.Add(
                "User-Agent", "CryptoPriceTracker/1.0 (+https://github.com/griverahn)");
    }

    public async Task<IReadOnlyDictionary<string,
        (decimal price, DateTime timestamp, string iconUrl)>> FetchPricesAsync(
            IEnumerable<CryptoAsset> assets,
            CancellationToken ct = default)
    {
        // Build the list of IDs required by CoinGecko
        var ids = string.Join(',', assets.Select(a => a.ExternalId));
        if (string.IsNullOrWhiteSpace(ids))
            return new Dictionary<string, (decimal, DateTime, string)>();

        // Complete endpoint
        var url = $"https://api.coingecko.com/api/v3/coins/markets" +
                $"?vs_currency=usd&ids={ids}&price_change_percentage=24h";

        // HTTP call
        using var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        // Deserialize JSON
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var data = await JsonSerializer.DeserializeAsync<List<CoinDto>>(stream, cancellationToken: ct)
                ?? new();

        // Map to output
        return data.ToDictionary(
            d => d.id,
            d => (d.current_price,
                d.last_updated.ToUniversalTime(),
                d.image),
            StringComparer.OrdinalIgnoreCase);
    }

    private sealed record CoinDto(
    string id,
    string name,
    string symbol,
    decimal current_price,
    string image,
    DateTime last_updated);

}
