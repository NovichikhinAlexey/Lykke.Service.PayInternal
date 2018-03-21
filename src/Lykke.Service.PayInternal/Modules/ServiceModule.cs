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
using Lykke.Service.PayInternal.AzureRepositories.Transaction;
using Lykke.Service.PayInternal.AzureRepositories.Wallet;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings;
using Lykke.Service.PayInternal.Mapping;
using Lykke.Service.PayInternal.Rabbit.Publishers;
using Lykke.Service.PayInternal.Services;
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
        // ReSharper disable once CollectionNeverUpdated.Local
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
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            RegisterAzureRepositories(builder);

            RegisterServiceClients(builder);

            RegisterAppServices(builder);

            RegisterCaches(builder);

            RegisterRabbitMqPublishers(builder);

            RegisterMapperValueResolvers(builder);

            builder.Populate(_services);
        }

        private void RegisterAzureRepositories(ContainerBuilder builder)
        {
            builder.RegisterInstance<IWalletRepository>(new WalletRepository(
                AzureTableStorage<WalletEntity>.Create(_dbSettings.ConnectionString(x => x.MerchantWalletConnString),
                    "MerchantWallets", _log)));

            builder.RegisterInstance<IPaymentRequestTransactionRepository>(new PaymentRequestTransactionRepository(
                AzureTableStorage<PaymentRequestTransactionEntity>.Create(
                    _dbSettings.ConnectionString(x => x.MerchantConnString),
                    "MerchantWalletTransactions", _log)));
        }

        private void RegisterAppServices(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance();

            builder.RegisterType<MerchantWalletsService>()
                .As<IMerchantWalletsService>()
                .SingleInstance();

            builder.RegisterType<AssetsAvailabilityService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.AssetsAvailability))
                .As<IAssetsAvailabilityService>();

            builder.RegisterType<CalculationService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.LpMarkup))
                .As<ICalculationService>()
                .SingleInstance();

            builder.RegisterType<TransactionsService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.TransactionConfirmationCount))
                .As<ITransactionsService>()
                .SingleInstance();

            builder.RegisterType<BtcTransferService>()
                .As<IBtcTransferService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.ExpirationPeriods))
                .SingleInstance();

            builder.RegisterType<TransferService>()
                .As<ITransferService>()
                .SingleInstance();

            builder.RegisterType<NoFeeProvider>()
                .As<IFeeProvider>()
                .SingleInstance();

            builder.RegisterType<BitcoinApiClient>()
                .Keyed<IBlockchainApiClient>(BlockchainType.Bitcoin)
                .SingleInstance();

            builder.RegisterType<RefundService>()
                .As<IRefundService>()
                .SingleInstance();
        }

        private void RegisterServiceClients(ContainerBuilder builder)
        {
            builder.RegisterBitcoinApiClient(_settings.CurrentValue.BitcoinCore.BitcoinCoreApiUrl);

            builder.RegisterInstance<IAssetsService>(
                new AssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl)));

            builder.RegisterType<LykkeMarketProfile>()
                .As<ILykkeMarketProfile>()
                .WithParameter("baseUri", new Uri(_settings.CurrentValue.MarketProfileServiceClient.ServiceUrl));

            builder.RegisterInstance(new QBitNinjaClient(_settings.CurrentValue.NinjaServiceClient.ServiceUrl))
                .AsSelf();
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

            builder.RegisterType<PaymentRequestPublisher>()
                .As<IPaymentRequestPublisher>()
                .As<IStartable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.Rabbit));

            builder.RegisterType<TransactionPublisher>()
                .As<ITransactionPublisher>()
                .As<IStartable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.Rabbit));
        }

        private void RegisterMapperValueResolvers(ContainerBuilder builder)
        {
            builder.RegisterType<TransactionUrlValueResolver>()
                .AsSelf()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.LykkeBlockchainExplorer))
                .SingleInstance();
        }
    }
}
