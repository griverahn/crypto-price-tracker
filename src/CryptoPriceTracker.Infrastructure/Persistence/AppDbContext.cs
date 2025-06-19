using Microsoft.EntityFrameworkCore;
using CryptoPriceTracker.Domain.Entities;

namespace CryptoPriceTracker.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<CryptoAsset> CryptoAssets => Set<CryptoAsset>();
    public DbSet<CryptoPriceHistory> CryptoPrices => Set<CryptoPriceHistory>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CryptoAsset>().HasData(
            new CryptoAsset { Id = 1, Name = "Bitcoin",  Symbol = "BTC", ExternalId = "bitcoin" },
            new CryptoAsset { Id = 2, Name = "Ethereum", Symbol = "ETH", ExternalId = "ethereum" }
        );

        // Unique index to avoid duplicate price entries for the same crypto asset on the same date
        modelBuilder.Entity<CryptoPriceHistory>()
            .HasIndex(p => new { p.CryptoAssetId, p.Date })
            .IsUnique();
    }
}

