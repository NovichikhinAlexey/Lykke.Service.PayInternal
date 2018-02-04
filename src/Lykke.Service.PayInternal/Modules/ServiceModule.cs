using System;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Lykke.Bitcoin.Api.Client;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.MarketProfile.Client;
using Lykke.Service.PayInternal.AzureRepositories.Merchant;
using Lykke.Service.PayInternal.AzureRepositories.Order;
using Lykke.Service.PayInternal.AzureRepositories.Transaction;
using Lykke.Service.PayInternal.AzureRepositories.Transfer;
using Lykke.Service.PayInternal.AzureRepositories.Wallet;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings;
using Lykke.Service.PayInternal.Services;
using Lykke.Service.PayInternal.Services.RabbitPublishers;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using QBitNinja.Client;
using DbSettings = Lykke.Service.PayInternal.Core.Settings.ServiceSettings.DbSettings;

namespace Lykke.Service.PayInternal.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly IReloadingManager<DbSettings> _dbSettings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _dbSettings = settings.Nested(x => x.PayInternalService.Db);
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // TODO: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            //  builder.RegisterType<QuotesPublisher>()
            //      .As<IQuotesPublisher>()
            //      .WithParameter(TypedParameter.From(_settings.CurrentValue.QuotesPublication))

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            RegisterAzureRepositories(builder);

            RegisterServiceClients(builder);

            RegisterAppServices(builder);

            RegisterCaches(builder);

            RegisterRabbitMqPublishers(builder);

            builder.Populate(_services);
        }

        private void RegisterAzureRepositories(ContainerBuilder builder)
        {
            builder.RegisterInstance<IWalletRepository>(new WalletRepository(
                AzureTableStorage<WalletEntity>.Create(_dbSettings.ConnectionString(x => x.MerchantWalletConnString),
                    "MerchantWallets", _log)));

            builder.RegisterInstance<IBlockchainTransactionRepository>(new BlockchainTransactionRepository(
                AzureTableStorage<BlockchainTransactionEntity>.Create(
                    _dbSettings.ConnectionString(x => x.MerchantConnString),
                    "MerchantWalletTransactions", _log)));

            builder.RegisterInstance<ITransferRepository>(new TransferRepository(
                AzureTableStorage<TransferEntity>.Create(_dbSettings.ConnectionString(x => x.TransferConnString),
                    "Transfers", _log)));
        }

        private void RegisterAppServices(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterType<MerchantWalletsService>()
                .As<IMerchantWalletsService>();

            builder.RegisterType<BtcTransferRequestService>()
                .As<ITransferRequestService>();

            builder.RegisterType<RatesCalculationService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.LpMarkup))
                .As<IRatesCalculationService>();

            builder.RegisterType<TransactionsService>()
                .As<ITransactionsService>();
        }

        private void RegisterServiceClients(ContainerBuilder builder)
        {
            builder.RegisterBitcoinApiClient(_settings.CurrentValue.BitcoinCore.BitcoinCoreApiUrl);

            builder.RegisterInstance<IAssetsService>(
                new AssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl)));

            builder.RegisterType<LykkeMarketProfile>()
                .As<ILykkeMarketProfile>()
                .WithParameter("baseUri", new Uri(_settings.CurrentValue.MarketProfileServiceClient.ServiceUrl));
            
        }

        private void RegisterCaches(ContainerBuilder builder)
        {
            builder.Register(x =>
            {
                var assetsService = x.Resolve<IComponentContext>().Resolve<IAssetsService>();

                return new CachedDataDictionary<string, Asset>
                (
                    async () => (await assetsService.AssetGetAllAsync()).ToDictionary(itm => itm.Id)
                );
            }).SingleInstance();

            builder.Register(x =>
            {
                var assetsService = x.Resolve<IComponentContext>().Resolve<IAssetsService>();

                return new CachedDataDictionary<string, AssetPair>(
                    async () => (await assetsService.AssetPairGetAllAsync()).ToDictionary(itm => itm.Id)
                );
            }).SingleInstance();

            builder.RegisterType<AssetsLocalCache>()
                .As<IAssetsLocalCache>();
        }

        private void RegisterRabbitMqPublishers(ContainerBuilder builder)
        {
            builder.RegisterType<WalletEventsPublisher>()
                .As<IWalletEventsPublisher>()
                .As<IStartable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.Rabbit));

            builder.RegisterType<TransactionsUpdatePublisher>()
                .As<ITransactionUpdatesPublisher>()
                .As<IStartable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.Rabbit));
        }
    }
}
