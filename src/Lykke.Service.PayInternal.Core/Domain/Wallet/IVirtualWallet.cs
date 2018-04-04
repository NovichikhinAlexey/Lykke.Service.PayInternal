using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IVirtualWallet
    {
        string MerchantId { get; set; }

        DateTime DueDate { get; set; }

        DateTime CreatedOn { get; set; }

        IEnumerable<OriginalWallet> OriginalWallets { get; set; }
    }
}
