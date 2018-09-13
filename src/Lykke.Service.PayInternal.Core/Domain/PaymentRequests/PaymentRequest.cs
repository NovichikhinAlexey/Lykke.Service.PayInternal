using System;

namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequests
{
    public class PaymentRequest : IPaymentRequest
    {
        public string Id { get; set; }

        public string MerchantId { get; set; }

        public string ExternalOrderId { get; set; }

        public string OrderId { get; set; }

        public decimal Amount { get; set; }

        public string SettlementAssetId { get; set; }

        public string PaymentAssetId { get; set; }

        public DateTime DueDate { get; set; }

        public double MarkupPercent { get; set; }

        public int MarkupPips { get; set; }

        public double MarkupFixedFee { get; set; }

        public string WalletAddress { get; set; }

        public PaymentRequestStatus Status { get; set; }

        public decimal PaidAmount { get; set; }

        public DateTime? PaidDate { get; set; }

        public PaymentRequestProcessingError ProcessingError { get; set; }

        public DateTime Timestamp { get; set; }

        public bool StatusValidForRefund()
        {
            return Status == PaymentRequestStatus.Error;
        }

        public bool StatusValidForPastDueTransition()
        {
            return Status == PaymentRequestStatus.New;
        }

        public bool StatusValidForSettlement()
        {
            return Status == PaymentRequestStatus.Confirmed ||
                   Status == PaymentRequestStatus.Error &&
                    (ProcessingError == PaymentRequestProcessingError.LatePaid ||
                     ProcessingError == PaymentRequestProcessingError.PaymentAmountAbove ||
                     ProcessingError == PaymentRequestProcessingError.PaymentAmountBelow ||
                     ProcessingError == PaymentRequestProcessingError.PaymentExpired);
        }

        public string Initiator { get; set; }

        public string SettlementErrorDescription { get; set; }
    }
}
