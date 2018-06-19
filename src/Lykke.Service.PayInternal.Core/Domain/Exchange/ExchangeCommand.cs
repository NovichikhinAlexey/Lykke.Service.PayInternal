using System;
using JetBrains.Annotations;

namespace Lykke.Service.PayInternal.Core.Domain.Exchange
{
    public class ExchangeCommand
    {
        public string MerchantId { get; set; }

        [CanBeNull] public string SourceMerchantWalletId { get; set; }

        public string SourceAssetId { get; set; }

        public decimal SourceAmount { get; set; }

        [CanBeNull] public string DestMerchantWalletId { get; set; }

        public string DestAssetId { get; set; }
    }
}
