namespace Lykke.Service.PayInternal.Core.Domain.Exchange
{
    public class PreExchangeCommand
    {
        public string MerchantId { get; set; }

        public string SourceAssetId { get; set; }

        public decimal SourceAmount { get; set; }

        public string DestAssetId { get; set; }
    }
}
