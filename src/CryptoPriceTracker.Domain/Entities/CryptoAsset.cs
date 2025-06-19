namespace CryptoPriceTracker.Domain.Entities
{
    public class CryptoAsset
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;        
        /// URL of the image returned by the API
        public string? IconUrl { get; set; }
        public ICollection<CryptoPriceHistory>? PriceHistory { get; set; }
    }
}

