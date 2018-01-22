using System;
using Lykke.Service.PayInternal.Core.Domain.Order;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class CreateOrder : ICreateOrder
    {
        public string MerchantId { get; set; }
        public string AssetPairId { get; set; }
        public string InvoiceAssetId { get; set; }
        public double InvoiceAmount { get; set; }
        public string ExchangeAssetId { get; set; }
        public double MarkupPercent { get; set; }
        public int MarkupPips { get; set; }
        public DateTime? WalletDueDate { get; set; }
        public string WalletAddress { get; set; }
    }
}
