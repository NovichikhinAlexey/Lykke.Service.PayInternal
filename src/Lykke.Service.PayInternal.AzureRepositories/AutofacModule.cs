using Autofac;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Lykke.Service.PayInternal.AzureRepositories.Asset;
using Lykke.Service.PayInternal.AzureRepositories.Markup;
using Lykke.Service.PayInternal.AzureRepositories.Order;
using Lykke.Service.PayInternal.AzureRepositories.PaymentRequest;
using Lykke.Service.PayInternal.AzureRepositories.Transaction;
using Lykke.Service.PayInternal.AzureRepositories.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.AzureRepositories.Wallet;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Domain.Orders;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.SettingsReader;
using Lykke.Service.PayInternal.AzureRepositories.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Domain.File;
using Lykke.Service.PayInternal.AzureRepositories.File;
using AzureStorage.Blob;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.AzureRepositories.AssetPair;
using Lykke.Service.PayInternal.AzureRepositories.MerchantWallet;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;

namespace Lykke.Service.PayInternal.AzureRepositories
{
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<string> _ordersConnectionString;
        private readonly IReloadingManager<string> _merchantsConnectionString;
        private readonly IReloadingManager<string> _paymentRequestsConnectionString;
        private readonly IReloadingManager<string> _transfersConnectionString;

        public AutofacModule(
            IReloadingManager<string> ordersConnectionString,
            IReloadingManager<string> merchantsConnectionString,
            IReloadingManager<string> paymentRequestsConnectionString,
            IReloadingManager<string> transfersConnectionString)
        {
            _ordersConnectionString = ordersConnectionString;
            _merchantsConnectionString = merchantsConnectionString;
            _paymentRequestsConnectionString = paymentRequestsConnectionString;
            _transfersConnectionString = transfersConnectionString;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            const string supervisorTableName = "Supervisors";
            const string paymentRequestsTableName = "PaymentRequests";
            const string ordersTableName = "Orders";
            const string transfersTableName = "Transfers";
            const string assetsAvailabilityTableName = "AssetsAvailability";
            const string assetsAvailabilityByMerchantTableName = "AssetsAvailabilityByMerchant";
            const string bcnWalletsUsageTableName = "BlockchainWalletsUsage";
            const string virtualWalletsTableName = "VirtualWallets";
            const string merchantTransactionsTableName = "MerchantWalletTransactions";
            const string markupsTableName = "Markups";
            const string merchantFilesTableName = "MerchantFiles";
            const string merchantWalletsTableName = "MerchantWallets";
            const string assetPairRatesTableName = "AssetPairRates";

            builder.Register(c => new SupervisorMembershipRepository(
                    AzureTableStorage<SupervisorMembershipEntity>.Create(_merchantsConnectionString,
                        supervisorTableName, c.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureIndex>.Create(_merchantsConnectionString, supervisorTableName,
                        c.Resolve<ILogFactory>())))
                .As<ISupervisorMembershipRepository>()
                .SingleInstance();

            builder.Register(c => new PaymentRequestRepository(
                    AzureTableStorage<PaymentRequestEntity>.Create(_paymentRequestsConnectionString,
                        paymentRequestsTableName, c.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureIndex>.Create(_paymentRequestsConnectionString,
                        paymentRequestsTableName, c.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureIndex>.Create(_paymentRequestsConnectionString,
                        paymentRequestsTableName, c.Resolve<ILogFactory>())))
                .As<IPaymentRequestRepository>()
                .SingleInstance();

            builder.Register(c => new VirtualWalletRepository(
                    AzureTableStorage<VirtualWalletEntity>.Create(_paymentRequestsConnectionString,
                        virtualWalletsTableName, c.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureIndex>.Create(_paymentRequestsConnectionString,
                        virtualWalletsTableName, c.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureIndex>.Create(_paymentRequestsConnectionString,
                        virtualWalletsTableName, c.Resolve<ILogFactory>())))
                .As<IVirtualWalletRepository>()
                .SingleInstance();

            builder.Register(c =>
                    new BcnWalletUsageRepository(AzureTableStorage<BcnWalletUsageEntity>.Create(
                        _paymentRequestsConnectionString, bcnWalletsUsageTableName, c.Resolve<ILogFactory>())))
                .As<IBcnWalletUsageRepository>()
                .SingleInstance();

            builder.Register(c =>
                    new OrdersRepository(
                        AzureTableStorage<OrderEntity>.Create(_ordersConnectionString, ordersTableName,
                            c.Resolve<ILogFactory>()),
                        AzureTableStorage<AzureIndex>.Create(_ordersConnectionString, ordersTableName,
                            c.Resolve<ILogFactory>())))
                .As<IOrderRepository>()
                .SingleInstance();

            builder.Register(c =>
                    new AssetGeneralSettingsRepository(AzureTableStorage<AssetGeneralSettingsEntity>.Create(
                        _paymentRequestsConnectionString, assetsAvailabilityTableName, c.Resolve<ILogFactory>())))
                .As<IAssetGeneralSettingsRepository>()
                .SingleInstance();

            builder.Register(c =>
                    new AssetMerchantSettingsRepository(AzureTableStorage<AssetMerchantSettingsEntity>.Create(
                        _paymentRequestsConnectionString, assetsAvailabilityByMerchantTableName,
                        c.Resolve<ILogFactory>())))
                .As<IAssetMerchantSettingsRepository>()
                .SingleInstance();

            builder.Register(c => new TransferRepository(
                    AzureTableStorage<TransferEntity>.Create(_transfersConnectionString, transfersTableName,
                        c.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureIndex>.Create(_transfersConnectionString, transfersTableName,
                        c.Resolve<ILogFactory>())))
                .As<ITransferRepository>()
                .SingleInstance();

            builder.Register(c => new PaymentRequestTransactionRepository(
                    AzureTableStorage<PaymentRequestTransactionEntity>.Create(_merchantsConnectionString,
                        merchantTransactionsTableName, c.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureIndex>.Create(_merchantsConnectionString, merchantTransactionsTableName,
                        c.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureIndex>.Create(_merchantsConnectionString, merchantTransactionsTableName,
                        c.Resolve<ILogFactory>())))
                .As<IPaymentRequestTransactionRepository>()
                .SingleInstance();

            builder.Register(c => new MarkupRepository(
                    AzureTableStorage<MarkupEntity>.Create(_merchantsConnectionString, markupsTableName,
                        c.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureIndex>.Create(_merchantsConnectionString, markupsTableName,
                        c.Resolve<ILogFactory>())))
                .As<IMarkupRepository>()
                .SingleInstance();

            builder.Register(c =>
                    new FileRepository(AzureBlobStorage.Create(_merchantsConnectionString)))
                .As<IFileRepository>()
                .SingleInstance();

            builder.Register(c =>
                    new FileInfoRepository(AzureTableStorage<FileInfoEntity>.Create(_merchantsConnectionString,
                        merchantFilesTableName, c.Resolve<ILogFactory>())))
                .As<IFileInfoRepository>()
                .SingleInstance();

            builder.Register(c => new MerchantWalletRepository(
                    AzureTableStorage<MerchantWalletEntity>.Create(_merchantsConnectionString, merchantWalletsTableName,
                        c.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureIndex>.Create(_merchantsConnectionString, merchantWalletsTableName,
                        c.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureIndex>.Create(_merchantsConnectionString, merchantWalletsTableName,
                        c.Resolve<ILogFactory>())))
                .As<IMerchantWalletRespository>()
                .SingleInstance();

            builder.Register(c => new AssetPairRateRepository(
                    AzureTableStorage<AssetPairRateEntity>.Create(_merchantsConnectionString, assetPairRatesTableName,
                        c.Resolve<ILogFactory>())))
                .As<IAssetPairRateRepository>()
                .SingleInstance();
        }
    }
}
