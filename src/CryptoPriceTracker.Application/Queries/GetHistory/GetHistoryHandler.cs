using MediatR;
using CryptoPriceTracker.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoPriceTracker.Application.Queries.GetHistory;

public class GetHistoryHandler
        : IRequestHandler<GetHistoryQuery, IReadOnlyList<HistoryPointDto>>
{
    private readonly ICryptoRepository _repo;
    public GetHistoryHandler(ICryptoRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<HistoryPointDto>> Handle(
        GetHistoryQuery q, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddDays(-q.Days);

        var points = await _repo.GetPriceHistoryAsync(
                        q.Symbol.ToUpper(), cutoff, ct);

        return points
              .OrderBy(p => p.Date)                    // asc order
              .Select(p => new HistoryPointDto(p.Date, p.Price))
              .ToList();
    }
}
