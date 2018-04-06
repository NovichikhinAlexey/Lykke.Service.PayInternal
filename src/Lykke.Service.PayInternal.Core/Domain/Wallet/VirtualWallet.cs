using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public class VirtualWallet : IVirtualWallet
    {
        public string Id { get; set; }
        public string MerchantId { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public IList<BlockchainWallet> BlockchainWallets { get; set; }
    }
}
