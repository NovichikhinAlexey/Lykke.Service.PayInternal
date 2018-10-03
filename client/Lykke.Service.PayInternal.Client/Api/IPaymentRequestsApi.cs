using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInternal.Client.Models.Validation;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IPaymentRequestsApi
    {
        [Get("/api/paymentrequests/paymentsFilter")]
        Task<GetByPaymentsFilterResponse> GetByPaymentsFilterAsync(
            string merchantId,
            [Query(CollectionFormat.Multi)] IEnumerable<string> statuses,
            [Query(CollectionFormat.Multi)] IEnumerable<string> processingErrors,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? take
        );

        [Get("/api/paymentrequests/hasAnyPaymentRequest/{merchantId}")]
        Task<bool> HasAnyPaymentRequestAsync(string merchantId);

        [Get("/api/merchants/{merchantId}/paymentrequests")]
        Task<IReadOnlyList<PaymentRequestModel>> GetAllAsync(string merchantId);
        
        [Get("/api/merchants/{merchantId}/paymentrequests/{paymentRequestId}")]
        Task<PaymentRequestModel> GetAsync(string merchantId, string paymentRequestId);

        [Get("/api/merchants/{merchantId}/paymentrequests/details/{paymentRequestId}")]
        Task<PaymentRequestDetailsModel> GetDetailsAsync(string merchantId, string paymentRequestId);

        [Get("/api/paymentrequests/byAddress/{walletAddress}")]
        Task<PaymentRequestModel> GetByAddressAsync(string walletAddress);
        
        [Post("/api/merchants/paymentrequests")]
        Task<PaymentRequestModel> CreateAsync([Body] CreatePaymentRequestModel model);
        
        [Post("/api/transfers/BtcFreeTransfer")]
        Task<BtcTransferResponse> BtcFreeTransferAsync([Body] BtcFreeTransferRequest request);

        [Post("/api/merchants/paymentrequests/refunds")]
        Task<RefundResponse> RefundAsync([Body] RefundRequestModel request);

        [Delete("/api/merchants/{merchantId}/paymentrequests/{paymentRequestId}")]
        Task CancelAsync(string merchantId, string paymentRequestId);

        [Post("/api/paymentrequests/payment")]
        Task PayAsync([Body] PaymentRequest request);

        [Post("/api/paymentrequests/prePayment")]
        Task PrePayAsync([Body] PrePaymentRequest request);

        [Get("/api/transfers/depositTransfer/validate")]
        Task<ValidateDepositTransferResult> ValidateDepositTransferAsync([Query] ValidateDepositTransferRequest request);
    }
}
