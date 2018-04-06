using System;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public class Wallet : IWallet
    {
        public string MerchantId { get; set; }
        public string Address { get; set; }
        public DateTime DueDate { get; set; }
        public double Amount { get; set; }
        public string PublicKey { get; set; }
    }
}
