using System;
using Autofac;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class AutofacModule: Module
    {
        private readonly TimeSpan _orderExpiration;
        private readonly int _transactionConfirmationCount;

        public AutofacModule(TimeSpan orderExpiration, int transactionConfirmationCount)
        {
            _orderExpiration = orderExpiration;
            _transactionConfirmationCount = transactionConfirmationCount;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MerchantService>()
                .As<IMerchantService>();

            builder.RegisterType<PaymentRequestService>()
                .As<IPaymentRequestService>()
                .WithParameter(TypedParameter.From(_transactionConfirmationCount));

            builder.RegisterType<OrderService>()
                .WithParameter(TypedParameter.From(_orderExpiration))
                .WithParameter(TypedParameter.From(_transactionConfirmationCount))
                .As<IOrderService>();
        }
    }
}
