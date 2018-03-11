﻿using Lykke.Service.PayInternal.Core.Domain.Refund;

namespace Lykke.Service.PayInternal.Models.Refunds
{
    public class RefundResponse : IRefund
    {
        public string RefundId { get; set; }
        public string MerchantId { get; set; }
        public string PaymentRequestId { get; set; }
        public string SettlementId { get; set; }
        public decimal Amount { get; set; }
    }
}
