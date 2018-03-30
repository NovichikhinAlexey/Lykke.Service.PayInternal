using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Client.Models.Asset
{
    public class AvailableAssetsByMerchantResponse
    {
        public string MerchantId { get; set; }
        public string PaymentAssets { get; set; }
        public string SettlementAssets { get; set; }
    }
}
