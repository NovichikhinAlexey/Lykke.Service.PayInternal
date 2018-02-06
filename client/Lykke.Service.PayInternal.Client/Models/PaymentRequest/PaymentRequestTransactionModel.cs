using System;

namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    public class PaymentRequestTransactionModel
    {
        public string Id { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
        public int Confirmations { get; set; }
        public string BlockId { get; set; }
        public DateTime FirstSeen { get; set; }
        public string Url { get; set; }
        public string RefundLink { get; set; }
    }
}
