using MediatR;
using System.Collections.Generic;

namespace CryptoPriceTracker.Application.Queries.GetHistory;

public record GetHistoryQuery(string Symbol, int Days)
        : IRequest<IReadOnlyList<HistoryPointDto>>;

public record HistoryPointDto(DateTime DateUtc, decimal Price);
