using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IVirtualWallet
    {
        string Id { get; set; }

        string MerchantId { get; set; }

        DateTime DueDate { get; set; }

        DateTime CreatedOn { get; set; }

        IList<BlockchainWallet> BlockchainWallets { get; set; }
    }
}
