using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IPaymentRequestsApi
    {
        [Get("/api/merchants/{merchantId}/paymentrequests")]
        Task<IReadOnlyList<PaymentRequestModel>> GetAllAsync(string merchantId);
        
        [Get("/api/merchants/{merchantId}/paymentrequests/{paymentRequestId}")]
        Task<PaymentRequestModel> GetAsync(string merchantId, string paymentRequestId);
        
        [Post("/api/merchants/paymentrequests")]
        Task<PaymentRequestModel> CreateAsync([Body] CreatePaymentRequestModel model);
    }
}
