using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Client.Models.Asset
{
    public class AssetByMerchantModel
    {
        public AssetAvailabilityType availabilityType { get; set; }
        public string MerchantId { get; set; }
    }
}
