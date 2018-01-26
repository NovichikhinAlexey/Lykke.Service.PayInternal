using System;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IWallet
    {
        string MerchantId { get; set; }
        string Address { get; set; }
        DateTime DueDate { get; set; }
        decimal Amount { get; set; }
        string PublicKey { get; set; }
    }
}
