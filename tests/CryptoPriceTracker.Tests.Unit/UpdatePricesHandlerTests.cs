using Xunit;
using Moq;
using CryptoPriceTracker.Application.Commands.UpdatePrices;
using CryptoPriceTracker.Application.Interfaces;

public class UpdatePricesHandlerTests
{
    [Fact]
    public async Task Handles_Duplicates_Correctly()
    {
        // Arrange
        var repo = new Mock<ICryptoRepository>();
        var fetcher = new Mock<IPriceFetcher>();

        // Setup mocks
        fetcher.Setup(f => f.FetchPricesAsync()).ReturnsAsync(new List<CryptoPrice>
        {
            new CryptoPrice { Id = 1, Price = 100 },
            new CryptoPrice { Id = 2, Price = 200 },
            new CryptoPrice { Id = 1, Price = 100 }
        });

        var handler = new UpdatePricesHandler(repo.Object, fetcher.Object);

        // Act
        var result = await handler.Handle(new UpdatePricesCommand(), default);

        // Assert
        Assert.True(result.Success);
        repo.Verify(r => r.SaveAsync(It.IsAny<IEnumerable<CryptoPrice>>()), Times.Once);
    }
}
