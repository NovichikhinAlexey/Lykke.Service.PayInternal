using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    /// <summary>
    /// Represents the response of payment requests by filter
    /// </summary>
    public class GetByPaymentsFilterResponse
    {
        public GetByPaymentsFilterResponse()
        {
            PaymeentRequests = new List<IPaymentRequest>();
        }

        /// <summary>
        /// The payment requests
        /// </summary>
        public IReadOnlyList<IPaymentRequest> PaymeentRequests { get; set; }
        
        /// <summary>
        /// The attribute of existance additional payment requests
        /// </summary>
        public bool HasMorePaymentRequests { get; set; }
    }
}
