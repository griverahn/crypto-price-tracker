using MediatR;

namespace CryptoPriceTracker.Application.Queries.GetLatestPrices;

public record GetLatestPricesQuery() : IRequest<IReadOnlyList<LatestPriceDto>>;

