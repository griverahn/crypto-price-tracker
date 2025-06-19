namespace CryptoPriceTracker.Application.Commands.UpdatePrices;

using MediatR;

public record UpdatePricesCommand() : IRequest<UpdatePricesResult>;

public record UpdatePricesResult(bool Success, int UpdatedCount, string? ErrorMessage);
