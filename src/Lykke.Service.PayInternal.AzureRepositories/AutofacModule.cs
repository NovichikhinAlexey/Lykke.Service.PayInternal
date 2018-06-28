using Autofac;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common.Log;
using Lykke.Service.PayInternal.AzureRepositories.Asset;
using Lykke.Service.PayInternal.AzureRepositories.Markup;
using Lykke.Service.PayInternal.AzureRepositories.Merchant;
using Lykke.Service.PayInternal.AzureRepositories.Order;
using Lykke.Service.PayInternal.AzureRepositories.PaymentRequest;
using Lykke.Service.PayInternal.AzureRepositories.Transaction;
using Lykke.Service.PayInternal.AzureRepositories.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.AzureRepositories.Wallet;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Orders;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.SettingsReader;
using Lykke.Service.PayInternal.AzureRepositories.MerchantGroup;
using Lykke.Service.PayInternal.AzureRepositories.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Domain.File;
using Lykke.Service.PayInternal.AzureRepositories.File;
using AzureStorage.Blob;
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
        private readonly ILog _log;

        public AutofacModule(
            IReloadingManager<string> ordersConnectionString,
            IReloadingManager<string> merchantsConnectionString,
            IReloadingManager<string> paymentRequestsConnectionString,
            IReloadingManager<string> transfersConnectionString,
            ILog log)
        {
            _ordersConnectionString = ordersConnectionString;
            _merchantsConnectionString = merchantsConnectionString;
            _paymentRequestsConnectionString = paymentRequestsConnectionString;
            _transfersConnectionString = transfersConnectionString;
            _log = log;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            const string merchantsTableName = "Merchants";
            const string merchantGroupsTableName = "MerchantGroups";
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

            builder.RegisterInstance<IMerchantRepository>(new MerchantRepository(
                AzureTableStorage<MerchantEntity>.Create(_merchantsConnectionString,
                    merchantsTableName, _log)));

            builder.RegisterInstance<IMerchantGroupRepository>(new MerchantGroupRepository(
                AzureTableStorage<MerchantGroupEntity>.Create(_merchantsConnectionString, merchantGroupsTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_merchantsConnectionString, merchantGroupsTableName, _log)));

            builder.RegisterInstance<ISupervisorMembershipRepository>(new SupervisorMembershipRepository(
                AzureTableStorage<SupervisorMembershipEntity>.Create(_merchantsConnectionString, supervisorTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_merchantsConnectionString, supervisorTableName, _log)));

            builder.RegisterInstance<IPaymentRequestRepository>(new PaymentRequestRepository(
                AzureTableStorage<PaymentRequestEntity>.Create(_paymentRequestsConnectionString,
                    paymentRequestsTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_paymentRequestsConnectionString,
                    paymentRequestsTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_paymentRequestsConnectionString, 
                    paymentRequestsTableName, _log)));

            builder.RegisterInstance<IVirtualWalletRepository>(new VirtualWalletRepository(
                AzureTableStorage<VirtualWalletEntity>.Create(_paymentRequestsConnectionString,
                    virtualWalletsTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_paymentRequestsConnectionString,
                    virtualWalletsTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_paymentRequestsConnectionString,
                    virtualWalletsTableName, _log)));

            builder.RegisterInstance<IBcnWalletUsageRepository>(new BcnWalletUsageRepository(
                AzureTableStorage<BcnWalletUsageEntity>.Create(_paymentRequestsConnectionString,
                    bcnWalletsUsageTableName, _log)));
            
            builder.RegisterInstance<IOrderRepository>(new OrdersRepository(
                AzureTableStorage<OrderEntity>.Create(_ordersConnectionString, ordersTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_ordersConnectionString, ordersTableName, _log)));

            builder.RegisterInstance<IAssetGeneralSettingsRepository>(new AssetGeneralSettingsRepository(
                AzureTableStorage<AssetGeneralSettingsEntity>.Create(_paymentRequestsConnectionString,
                    assetsAvailabilityTableName, _log)));

            builder.RegisterInstance<IAssetMerchantSettingsRepository>(new AssetMerchantSettingsRepository(
                AzureTableStorage<AssetMerchantSettingsEntity>.Create(_paymentRequestsConnectionString,
                    assetsAvailabilityByMerchantTableName, _log)));

            builder.RegisterInstance<ITransferRepository>(new TransferRepository(
                AzureTableStorage<TransferEntity>.Create(_transfersConnectionString, transfersTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_transfersConnectionString, transfersTableName, _log)));

            builder.RegisterInstance<IPaymentRequestTransactionRepository>(new PaymentRequestTransactionRepository(
                AzureTableStorage<PaymentRequestTransactionEntity>.Create(_merchantsConnectionString, merchantTransactionsTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_merchantsConnectionString, merchantTransactionsTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_merchantsConnectionString, merchantTransactionsTableName, _log)));

            builder.RegisterInstance<IMarkupRepository>(new MarkupRepository(
                AzureTableStorage<MarkupEntity>.Create(_merchantsConnectionString, markupsTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_merchantsConnectionString, markupsTableName, _log)));

            builder.RegisterInstance<IFileRepository>(
                new FileRepository(AzureBlobStorage.Create(_merchantsConnectionString)));

            builder.RegisterInstance<IFileInfoRepository>(
                new FileInfoRepository(AzureTableStorage<FileInfoEntity>.Create(_merchantsConnectionString,
                    merchantFilesTableName, _log)));

            builder.RegisterInstance<IMerchantWalletRespository>(new MerchantWalletRepository(
                AzureTableStorage<MerchantWalletEntity>.Create(_merchantsConnectionString, merchantWalletsTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_merchantsConnectionString, merchantWalletsTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_merchantsConnectionString, merchantWalletsTableName, _log)));

            builder.RegisterInstance<IAssetPairRateRepository>(new AssetPairRateRepository(
                AzureTableStorage<AssetPairRateEntity>.Create(_merchantsConnectionString, assetPairRatesTableName,
                    _log)));
        }
    }
}
