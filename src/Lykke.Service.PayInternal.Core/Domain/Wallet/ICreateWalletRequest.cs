using System;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface ICreateWalletRequest
    {
        string MerchantId { get; set; }

        DateTime DueDate { get; set; }
    }
}
