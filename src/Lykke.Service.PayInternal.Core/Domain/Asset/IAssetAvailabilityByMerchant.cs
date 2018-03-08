using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Asset
{
    public interface IAssetAvailabilityByMerchant
    {
        string MerchantId { get; set; }
        string AssetsPayment { get; set; }
        string AssetsSettlement { get; set; }
    }
}
