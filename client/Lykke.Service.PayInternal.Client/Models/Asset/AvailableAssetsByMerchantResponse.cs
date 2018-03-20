using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Client.Models.Asset
{
    public class AvailableAssetsByMerchantResponse
    {
        public string MerchantId { get; set; }
        public string AssetsPayment { get; set; }
        public string AssetsSettlement { get; set; }
    }
}
