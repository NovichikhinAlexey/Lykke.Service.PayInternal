using System;

namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequest
{
    public class PaymentRequestStatusInfo
    {
        public PaymentRequestStatus Status { get; set; }

        public string Details { get; set; }

        public decimal Amount { get; set; }

        public DateTime? Date { get; set; }

        public static PaymentRequestStatusInfo Confirmed(decimal paid, DateTime date)
        {
            return new PaymentRequestStatusInfo
            {
                Status = PaymentRequestStatus.Confirmed,
                Amount = paid,
                Date =  date
            };
        }

        public static PaymentRequestStatusInfo Error(string errorMessage, decimal paid, DateTime? date)
        {
            return new PaymentRequestStatusInfo
            {
                Status = PaymentRequestStatus.Error,
                Details = errorMessage,
                Amount = paid,
                Date = date
            };
        }

        public static PaymentRequestStatusInfo InProcess()
        {
            return new PaymentRequestStatusInfo
            {
                Status = PaymentRequestStatus.InProcess
            };
        }

        public static PaymentRequestStatusInfo New()
        {
            return new PaymentRequestStatusInfo
            {
                Status = PaymentRequestStatus.New
            };
        }

        public static PaymentRequestStatusInfo None()
        {
            return new PaymentRequestStatusInfo
            {
                Status = PaymentRequestStatus.None
            };
        }
    }
}
