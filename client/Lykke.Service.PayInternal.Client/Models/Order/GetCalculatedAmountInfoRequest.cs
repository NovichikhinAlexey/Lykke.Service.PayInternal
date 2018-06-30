using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Client.Models.Order
{
    public class GetCalculatedAmountInfoRequest
    {
        public string SettlementAssetId { get; set; }
        public string PaymentAssetId { get; set; }
        public decimal Amount { get; set; }
        public string MerchantId { get; set; }
    }
}
