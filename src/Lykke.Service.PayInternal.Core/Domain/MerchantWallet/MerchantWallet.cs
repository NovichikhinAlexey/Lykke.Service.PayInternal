using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.MerchantWallet
{
    public class MerchantWallet : IMerchantWallet
    {
        public string Id { get; set; }
        public string MerchantId { get; set; }
        public BlockchainType Network { get; set; }
        public string WalletAddress { get; set; }
        public string DisplayName { get; set; }
        public DateTime CreatedOn { get; set; }
        public IList<string> IncomingPaymentDefaults { get; set; }
        public IList<string> OutgoingPaymentDefaults { get; set; }
    }
}
