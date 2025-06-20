using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CryptoPriceTracker.Application.Commands.UpdatePrices;
using CryptoPriceTracker.Application.Queries.GetLatestPrices;
using CryptoPriceTracker.Application.Queries.GetHistory;

namespace CryptoPriceTracker.Api.Controllers
{
    [ApiController]
    [Route("api/crypto")]
    public class CryptoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CryptoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Triggers the price update:
        /// 1. Queries CoinGecko via IPriceFetcher.
        /// 2. Validates deduplicates.
        /// 3. Persists in the database via ICryptoRepository.
        /// </summary>
        [HttpPost("update-prices")]
        public async Task<IActionResult> UpdatePrices()
        {
            var result = await _mediator.Send(new UpdatePricesCommand());

            return Ok(new
            {
                success = result.Success,
                updatedCount = result.Inserted,
                errorMessage = result.Error
            });
        }

        /// <summary>
        /// Returns the latest saved price by asset,
        /// including trend and icon URL.
        /// Consumed by the Razor view (Index.cshtml).
        /// </summary>
        [HttpGet("latest-prices")]
        public async Task<IActionResult> GetLatestPrices()
        {
            var latest = await _mediator.Send(new GetLatestPricesQuery());
            return Ok(latest);
        }
        
        /// <summary>
        /// Returns the price history for a specific asset symbol.
        /// </summary>
        [HttpGet("history/{symbol}")]
        public async Task<IActionResult> GetHistory(
                string symbol, [FromQuery] int days = 30)
        {
            var query  = new GetHistoryQuery(symbol, days);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
