using MediatR;

namespace CryptoPriceTracker.Application.Commands.UpdatePrices;

public record UpdatePricesCommand() : IRequest<UpdatePricesResult>;
