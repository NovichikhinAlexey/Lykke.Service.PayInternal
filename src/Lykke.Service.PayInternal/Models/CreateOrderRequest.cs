using Lykke.Service.PayInternal.Core.Domain.Order;

namespace Lykke.Service.PayInternal.Models
{
    public class CreateOrderRequest : ICreateOrderRequest
    {
        public string MerchantId { get; set; }

        public string InvoiceId { get; set; }

        public string AssetPairId { get; set; }

        public string InvoiceAssetId { get; set; }

        public double InvoiceAmount { get; set; }

        public string ExchangeAssetId { get; set; }

        public double MarkupPercent { get; set; }

        public int MarkupPips { get; set; }

        public string SuccessUrl { get; set; }

        public string ErrorUrl { get; set; }

        public string ProgressUrl { get; set; }

        public string WalletAddress { get; set; }
    }
}
