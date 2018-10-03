using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequests
{
    public class PaymentsFilter
    {
        public PaymentsFilter()
        {
            Statuses = new List<PaymentRequestStatus>();
            ProcessingErrors = new List<PaymentRequestProcessingError>();
        }

        public string MerchantId { get; set; }
        public IEnumerable<PaymentRequestStatus> Statuses { get; set; }
        public IEnumerable<PaymentRequestProcessingError> ProcessingErrors { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
