using System;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.Models
{
    public class CreateWalletRequest : ICreateWalletRequest
    {
        public string MerchantId { get; set; }
        public DateTime DueDate { get; set; }
    }
}
