using System;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Refit;

namespace Lykke.Service.PayInternal.Client.Exceptions
{
    internal static class ExceptionFactories
    {
        public static Func<RefundError, ApiException, RefundErrorResponseException> CreateRefundException =>
            (error, apiException) => new RefundErrorResponseException(error, apiException);

        public static Func<ErrorResponse, ApiException, DefaultErrorResponseException> CreateDefaultException =>
            (error, apiException) => new DefaultErrorResponseException(error, apiException);

        public static Func<CreatePaymentRequestError, ApiException, CreatePaymentRequestResponseException> CreatePaymentRequestException =>
            (error, apiException) => new CreatePaymentRequestResponseException(error, apiException);
    }
}
