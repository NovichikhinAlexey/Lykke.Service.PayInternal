using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.MerchantWallet
{
    public interface IMerchantWallet
    {
        string Id { get; set; }

        string MerchantId { get; set; }

        BlockchainType Network { get; set; }

        string WalletAddress { get; set; }

        string DisplayName { get; set; }

        DateTime CreatedOn { get; set; }

        IList<string> IncomingPaymentDefaults { get; set; }

        IList<string> OutgoingPaymentDefaults { get; set; }
    }
}
