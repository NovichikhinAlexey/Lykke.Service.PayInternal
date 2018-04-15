using System;
using System.Collections.Generic;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class WalletState : IWalletState
    {
        public string Address { get; set; }

        public DateTime DueDate { get; set; }

        public BlockchainType Blockchain { get; set; }

        public IEnumerable<IPaymentRequestTransaction> Transactions { get; set; }
    }
}
