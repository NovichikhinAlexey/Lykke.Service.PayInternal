using System;

namespace Lykke.Service.PayInternal.Client.Models.Wallets
{
    public class CreateWalletRequest
    {
        public string MerchantId { get; set; }
        public DateTime DueDate { get; set; }
    }
}
