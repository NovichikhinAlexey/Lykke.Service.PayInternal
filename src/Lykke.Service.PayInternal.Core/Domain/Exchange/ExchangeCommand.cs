using JetBrains.Annotations;

namespace Lykke.Service.PayInternal.Core.Domain.Exchange
{
    public class ExchangeCommand: PreExchangeCommand
    {
        [CanBeNull] public string SourceMerchantWalletId { get; set; }

        [CanBeNull] public string DestMerchantWalletId { get; set; }

        public decimal? ExpectedRate { get; set; }
    }
}
