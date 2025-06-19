namespace CryptoPriceTracker.Application.Commands.UpdatePrices;

public record UpdatePricesResult(bool Success, int Inserted, string? Error = null);
