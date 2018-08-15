using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    public class PaymentRequestModel
    {
        public string Id { get; set; }

        public string MerchantId { get; set; }

        public decimal Amount { get; set; }

        public string OrderId { get; set; }
        public string ExternalOrderId { get; set; }

        public string SettlementAssetId { get; set; }

        public string PaymentAssetId { get; set; }

        public DateTime DueDate { get; set; }

        public double MarkupPercent { get; set; }

        public int MarkupPips { get; set; }

        public double MarkupFixedFee { get; set; }

        public string WalletAddress { get; set; }

        [EnumDataType(typeof(PaymentRequestStatus), ErrorMessage = "Invalid value, possible values are: None, New, Cancelled, InProcess, Confirmed, RefundInProgress, Refunded, Error")]
        public PaymentRequestStatus Status { get; set; }

        public decimal PaidAmount { get; set; }

        public DateTime? PaidDate { get; set; }

        [EnumDataType(typeof(PaymentRequestProcessingError), ErrorMessage = "Invalid value, possible values are: None, UnknownRefund, UnknownPayment, PaymentAmountAbove, PaymentAmountBelow, PaymentAmountBelow, RefundNotConfirmed, LatePaid")]
        public PaymentRequestProcessingError ProcessingError { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
