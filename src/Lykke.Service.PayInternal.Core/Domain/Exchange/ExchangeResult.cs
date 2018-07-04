namespace Lykke.Service.PayInternal.Core.Domain.Exchange
{
    public class ExchangeResult
    {
        public string SourceAssetId { get; set; }

        public decimal SourceAmount { get; set; }

        public string DestAssetId { get; set; }

        public decimal DestAmount { get; set; }

        public decimal Rate { get; set; }
    }
}
