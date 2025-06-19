using Xunit;
using Moq;
using CryptoPriceTracker.Application.Commands.UpdatePrices;
using CryptoPriceTracker.Application.Interfaces;
using CryptoPriceTracker.Domain.Entities;

public class UpdatePricesHandlerTests
{
    [Fact]
    public async Task Inserts_only_non_duplicate_prices()
    {
        // Arrange
        var assets = new List<CryptoAsset>
        {
            new() { Id = 1, Name = "Bitcoin", Symbol = "BTC", ExternalId = "bitcoin" }
        };

        var repo = new Mock<ICryptoRepository>();
        repo.Setup(r => r.GetAssetsAsync()).ReturnsAsync(assets);
        repo.Setup(r => r.AddPricesAsync(It.IsAny<IEnumerable<CryptoPrice>>(), default))
            .Returns(Task.CompletedTask);

        var fetcher = new Mock<IPriceFetcher>();
        fetcher.Setup(f => f.FetchPricesAsync(It.IsAny<IEnumerable<CryptoAsset>>(), default))
               .ReturnsAsync(new Dictionary<string, (decimal, DateTime, string)>
               {
                   { "bitcoin", (60_000m, DateTime.UtcNow, "https://img") }
               });

        var handler = new UpdatePricesHandler(repo.Object, fetcher.Object);

        // Act
        var result = await handler.Handle(new UpdatePricesCommand(), default);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.Inserted);
        repo.Verify(r => r.AddPricesAsync(It.IsAny<IEnumerable<CryptoPriceHistory>>(), default),
                    Times.Once);
    }
}
