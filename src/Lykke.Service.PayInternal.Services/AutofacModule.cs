using System.Collections.Generic;
using Autofac;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class AutofacModule: Module
    {
        private readonly ExpirationPeriodsSettings _expirationPeriods;
        private readonly int _transactionConfirmationCount;
        private readonly IList<BlockchainWalletAllocationPolicy> _walletAllocationSettings;

        public AutofacModule(
            ExpirationPeriodsSettings expirationPeriods,
            int transactionConfirmationCount,
            IList<BlockchainWalletAllocationPolicy> walletAllocationSettings)
        {
            _expirationPeriods = expirationPeriods;
            _transactionConfirmationCount = transactionConfirmationCount;
            _walletAllocationSettings = walletAllocationSettings;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MerchantService>()
                .As<IMerchantService>();

            builder.RegisterType<PaymentRequestService>()
                .As<IPaymentRequestService>()
                .WithParameter(TypedParameter.From(_expirationPeriods));

            builder.RegisterType<RefundService>()
                .As<IRefundService>()
                .WithParameter(TypedParameter.From(_expirationPeriods.Refund));

            builder.RegisterType<OrderService>()
                .WithParameter(TypedParameter.From(_expirationPeriods.Order))
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

            builder.RegisterType<BlockchainClientProvider>()
                .As<IBlockchainClientProvider>();

            builder.RegisterType<PaymentRequestDetailsBuilder>()
                .As<IPaymentRequestDetailsBuilder>();
            builder.RegisterType<FileService>()
                .As<IFileService>();
        }
    }
}
