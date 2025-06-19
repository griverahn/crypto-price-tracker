namespace CryptoPriceTracker.Application.Queries.GetLatestPrices;

using MediatR;

public record GetLatestPricesQuery() : IRequest<IReadOnlyList<LatestPriceDto>>;

