namespace CryptoPriceTracker.Domain.Entities
{
    public class CryptoPriceHistory
    {
        public int Id { get; set; }
        public int CryptoAssetId { get; set; }
        public CryptoAsset CryptoAsset { get; set; } = new CryptoAsset();
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
    }
}
