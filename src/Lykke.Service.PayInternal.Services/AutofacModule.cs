using System;
using Autofac;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class AutofacModule: Module
    {
        private readonly TimeSpan _orderExpiration;

        public AutofacModule(TimeSpan orderExpiration)
        {
            _orderExpiration = orderExpiration;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MerchantService>()
                .As<IMerchantService>();

            builder.RegisterType<PaymentRequestService>()
                .As<IPaymentRequestService>();

            builder.RegisterType<OrderService>()
                .WithParameter(TypedParameter.From(_orderExpiration))
                .As<IOrderService>();
        }
    }
}
