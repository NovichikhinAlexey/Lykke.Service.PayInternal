using System;
using System.Collections.Generic;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IWalletState
    {
        string Address { get; set; }
        DateTime DueDate { get; set; }
        IEnumerable<IBlockchainTransaction> Transactions { get; set; }
    }
}
