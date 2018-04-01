using System;
using Autofac;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class AutofacModule: Module
    {
        private readonly TimeSpan _orderExpiration;
        private readonly TimeSpan _refundExpiration;
        private readonly int _transactionConfirmationCount;

        public AutofacModule(
            TimeSpan orderExpiration,
            TimeSpan refundExpiration,
            int transactionConfirmationCount)
        {
            _orderExpiration = orderExpiration;
            _refundExpiration = refundExpiration;
            _transactionConfirmationCount = transactionConfirmationCount;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MerchantService>()
                .As<IMerchantService>();

            builder.RegisterType<PaymentRequestService>()
                .As<IPaymentRequestService>()
                .WithParameter(TypedParameter.From(_transactionConfirmationCount));

            builder.RegisterType<RefundService>()
                .As<IRefundService>()
                .WithParameter(TypedParameter.From(_refundExpiration));

            builder.RegisterType<OrderService>()
                .WithParameter(TypedParameter.From(_orderExpiration))
                .As<IOrderService>();

            builder.RegisterType<PaymentRequestStatusResolver>()
                .WithParameter(TypedParameter.From(_transactionConfirmationCount))
                .As<IPaymentRequestStatusResolver>();
        }
    }
}
