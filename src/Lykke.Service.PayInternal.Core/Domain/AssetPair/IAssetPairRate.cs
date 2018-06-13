using System;

namespace Lykke.Service.PayInternal.Core.Domain.AssetPair
{
    public interface IAssetPairRate
    {
        string BaseAssetId { get; set; }

        string QuotingAssetId { get; set; }

        decimal BidPrice { get; set; }

        decimal AskPrice { get; set; }

        DateTime CreatedOn { get; set; }
    }
}
