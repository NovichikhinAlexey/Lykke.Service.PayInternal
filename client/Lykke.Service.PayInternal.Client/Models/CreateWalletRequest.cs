using System;

namespace Lykke.Service.PayInternal.Client.Models
{
    public class CreateWalletRequest
    {
        public string MerchantId { get; set; }
        public DateTime DueDate { get; set; }
    }
}
