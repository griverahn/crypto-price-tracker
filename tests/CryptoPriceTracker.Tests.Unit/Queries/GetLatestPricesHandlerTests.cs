using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading;
using CryptoPriceTracker.Application.Queries.GetLatestPrices;
using CryptoPriceTracker.Application.Interfaces;
using CryptoPriceTracker.Domain.Entities;

public class GetLatestPricesHandlerTests
{
    [Fact]
    public async void Returns_zero_price_when_no_history()
    {
        // Arrange
        var assets = new List<CryptoAsset>
        {
            new() { Id = 1, Name = "Bitcoin", Symbol = "BTC", ExternalId = "bitcoin" }
        };

        var repo = new Mock<ICryptoRepository>();
        repo.Setup(r => r.GetAssetsAsync())
            .ReturnsAsync(assets);

        var handler = new GetLatestPricesHandler(repo.Object);

        // Act
        var list = await handler.Handle(new GetLatestPricesQuery(), CancellationToken.None);

        // Assert
        Assert.Single(list);
        Assert.Equal(0, list[0].Price);
        Assert.Equal("➖", list[0].Trend);
    }
}
