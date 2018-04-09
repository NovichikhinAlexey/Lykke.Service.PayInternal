using System;
using Autofac;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class AutofacModule: Module
    {
        private readonly ExpirationPeriodsSettings _expirationPeriods;
        private readonly int _transactionConfirmationCount;

        public AutofacModule(
            ExpirationPeriodsSettings expirationPeriods,
            int transactionConfirmationCount)
        {
            _expirationPeriods = expirationPeriods;
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
                .WithParameter(TypedParameter.From(_expirationPeriods.Refund));

            builder.RegisterType<OrderService>()
                .WithParameter(TypedParameter.From(_expirationPeriods.Order))
                .As<IOrderService>();

            builder.RegisterType<PaymentRequestStatusResolver>()
                .WithParameter(TypedParameter.From(_transactionConfirmationCount))
                .As<IPaymentRequestStatusResolver>();
        }
    }
}
