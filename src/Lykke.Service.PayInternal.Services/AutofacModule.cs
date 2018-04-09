using System;
using System.Collections.Generic;
using Autofac;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class AutofacModule: Module
    {
        private readonly TimeSpan _orderExpiration;
        private readonly TimeSpan _refundExpiration;
        private readonly int _transactionConfirmationCount;
        private readonly IList<BlockchainWalletAllocationPolicy> _walletAllocationSettings;

        public AutofacModule(
            TimeSpan orderExpiration,
            TimeSpan refundExpiration,
            int transactionConfirmationCount,
            IList<BlockchainWalletAllocationPolicy> walletAllocationSettings)
        {
            _orderExpiration = orderExpiration;
            _refundExpiration = refundExpiration;
            _transactionConfirmationCount = transactionConfirmationCount;
            _walletAllocationSettings = walletAllocationSettings;
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

            builder.RegisterType<BcnWalletUsageService>()
                .As<IBcnWalletUsageService>();

            builder.RegisterType<VirtualWalletService>()
                .As<IVirtualWalletService>();

            builder.RegisterType<WalletManager>()
                .WithParameter(TypedParameter.From(_walletAllocationSettings))
                .As<IWalletManager>();

            builder.RegisterType<TransactionsManager>()
                .As<ITransactionsManager>();
        }
    }
}
