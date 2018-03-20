using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Asset
{
    public class AssetAvailabilityByMerchant : IAssetAvailabilityByMerchant
    {
        public string MerchantId { get; set; }
        public string AssetsPayment { get; set; }
        public string AssetsSettlement { get; set; }
    }
}
