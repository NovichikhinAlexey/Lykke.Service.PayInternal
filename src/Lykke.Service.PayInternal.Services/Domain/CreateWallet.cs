using System;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class CreateWallet : ICreateWalletRequest
    {
        public string MerchantId { get; set; }
        public DateTime DueDate { get; set; }
    }
}
