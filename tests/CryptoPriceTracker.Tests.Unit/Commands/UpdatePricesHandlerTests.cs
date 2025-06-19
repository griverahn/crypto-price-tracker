using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using CryptoPriceTracker.Application.Commands.UpdatePrices;
using CryptoPriceTracker.Application.Interfaces;
using CryptoPriceTracker.Domain.Entities;

public class UpdatePricesHandlerTests
{
    [Fact]
    public async void Ignores_duplicate_timestamp()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var asset = new CryptoAsset
        {
            Id = 1, Name = "Bitcoin", Symbol = "BTC", ExternalId = "bitcoin",
            PriceHistory = new List<CryptoPriceHistory>
            {
                new() { CryptoAssetId = 1, Date = now, Price = 100m }
            }
        };

        var repo = new Mock<ICryptoRepository>();
        repo.Setup(r => r.GetAssetsAsync()).ReturnsAsync(new List<CryptoAsset> { asset });
        repo.Setup(r => r.AddPricesAsync(It.IsAny<IEnumerable<CryptoPriceHistory>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var fetcher = new Mock<IPriceFetcher>();
        fetcher.Setup(f => f.FetchPricesAsync(It.IsAny<IEnumerable<CryptoAsset>>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new Dictionary<string, (decimal, DateTime, string)>
               {
                   { "bitcoin", (110m, now, string.Empty) }   // same timestamp as existing
               });

        var handler = new UpdatePricesHandler(repo.Object, fetcher.Object);

        // Act
        var result = await handler.Handle(new UpdatePricesCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.Inserted);   // no records inserted
        repo.Verify(r => r.AddPricesAsync(It.IsAny<IEnumerable<CryptoPriceHistory>>(), It.IsAny<CancellationToken>()),
                    Times.Never);
    }
}
