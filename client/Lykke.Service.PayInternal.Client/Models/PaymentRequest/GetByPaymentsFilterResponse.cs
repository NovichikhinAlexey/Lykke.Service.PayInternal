using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    /// <summary>
    /// Represents the response of payment requests by filter
    /// </summary>
    public class GetByPaymentsFilterResponse
    {
        /// <summary>
        /// Constructor of the class
        /// </summary>
        public GetByPaymentsFilterResponse() => PaymeentRequests = new List<PaymentRequestModel>();

        /// <summary>
        /// The payment requests
        /// </summary>
        public IReadOnlyList<PaymentRequestModel> PaymeentRequests { get; set; }
        
        /// <summary>
        /// The attribute of existance additional payment requests
        /// </summary>
        public bool HasMorePaymentRequests { get; set; }
    }
}
