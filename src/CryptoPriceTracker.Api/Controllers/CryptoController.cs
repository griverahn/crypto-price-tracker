using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CryptoPriceTracker.Application.Commands.UpdatePrices;
using CryptoPriceTracker.Application.Queries.GetLatestPrices;

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
        /// Dispara la actualización de precios:
        /// 1. Consulta CoinGecko a través del IPriceFetcher.
        /// 2. Valida deduplicados.
        /// 3. Persiste en la base de datos mediante ICryptoRepository.
        /// </summary>
        [HttpPost("update-prices")]
        public async Task<IActionResult> UpdatePrices()
        {
            var result = await _mediator.Send(new UpdatePricesCommand());

            // Puedes devolver un DTO más rico si tu handler lo provee
            return result.Success
                ? Ok(new { message = "Prices updated.", updated = result.UpdatedCount })
                : StatusCode(500, result.ErrorMessage);
        }

        /// <summary>
        /// Devuelve el último precio guardado por activo,
        /// inclu­yendo tendencia y URL del icono.
        /// Consumido por la vista Razor (Index.cshtml).
        /// </summary>
        [HttpGet("latest-prices")]
        public async Task<IActionResult> GetLatestPrices()
        {
            var latest = await _mediator.Send(new GetLatestPricesQuery());
            return Ok(latest);
        }
    }
}
