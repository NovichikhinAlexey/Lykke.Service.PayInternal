using System;
using System.Collections.Generic;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IWalletState
    {
        string Address { get; set; }

        DateTime DueDate { get; set; }

        BlockchainType Blockchain { get; set; }

        IEnumerable<IPaymentRequestTransaction> Transactions { get; set; }
    }
}
