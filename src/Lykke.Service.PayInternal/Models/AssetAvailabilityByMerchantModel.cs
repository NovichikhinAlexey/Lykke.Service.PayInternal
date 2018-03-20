using Lykke.Service.PayInternal.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Models
{
    public class AssetAvailabilityByMerchantModel
    {
        public AssetAvailabilityType availabilityType { get; set; }
        public string MerchantId { get; set; }
    }
}
