using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Common.Log;
using Lykke.Bitcoin.Api.Client;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.EthereumCore.Client;
using Lykke.Service.MarketProfile.Client;
using Lykke.Service.PayCallback.Client;
using Lykke.Service.PayHistory.Client;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings;
using Lykke.Service.PayInternal.Mapping;
using Lykke.Service.PayInternal.PeriodicalHandlers;
using Lykke.Service.PayInternal.Rabbit.Publishers;
using Lykke.Service.PayInternal.Services;
using Lykke.Service.PayInternal.Services.Mapping;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Polly;
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

            RegisterServiceClients(builder);

            RegisterAppServices(builder);

            RegisterCaches(builder);

            RegisterRabbitMqPublishers(builder);

            RegisterMapperValueResolvers(builder);

            RegisterPeriodicalHandlers(builder);

            builder.Populate(_services);
        }

        private void RegisterAppServices(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue))
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance();

            builder.RegisterType<AssetSettingsService>()
                .As<IAssetSettingsService>();

            builder.RegisterType<SupervisorMembershipService>()
                .As<ISupervisorMembershipService>();

            builder.RegisterType<MerchantGroupService>()
                .As<IMerchantGroupService>();

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
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.Blockchain.Bitcoin.Network))
                .SingleInstance();

            builder.RegisterType<BlockchainAddressValidator>()
                .As<IBlockchainAddressValidator>();

            builder.RegisterType<EthereumApiClient>()
                .Keyed<IBlockchainApiClient>(BlockchainType.Ethereum)
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.Blockchain.Ethereum))
                .SingleInstance();

            builder.RegisterType<EthereumIataApiClient>()
                .Keyed<IBlockchainApiClient>(BlockchainType.EthereumIata)
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.Blockchain.Ethereum))
                .SingleInstance();

            builder.RegisterType<LykkeAssetsResolver>()
                .As<ILykkeAssetsResolver>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.AssetsMap));

            builder.RegisterType<MarkupService>()
                .As<IMarkupService>();

            builder.RegisterType<BcnSettingsResolver>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.Blockchain))
                .As<IBcnSettingsResolver>();

            builder.RegisterType<FileService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.Merchant))
                .As<IFileService>();

            builder.RegisterType<WalletHistoryService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.RetryPolicy))
                .As<IWalletHistoryService>();

            builder.RegisterType<CashoutService>()
                .As<ICashoutService>();

            builder.RegisterType<WalletBalanceValidator>()
                .As<IWalletBalanceValidator>();

            builder.RegisterType<ConfirmationsService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.RetryPolicy))
                .As<IConfirmationsService>();
        }

        private void RegisterServiceClients(ContainerBuilder builder)
        {
            builder.RegisterBitcoinApiClient(_settings.CurrentValue.BitcoinCore.BitcoinCoreApiUrl);

            builder.RegisterInstance<IAssetsService>(
                new AssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl)));

            builder.RegisterType<LykkeMarketProfile>()
                .As<ILykkeMarketProfile>()
                .WithParameter("baseUri", new Uri(_settings.CurrentValue.MarketProfileServiceClient.ServiceUrl));

            builder.RegisterInstance<IEthereumCoreAPI>(
                new EthereumCoreAPI(new Uri(_settings.CurrentValue.EthereumServiceClient.ServiceUrl)));

            builder.RegisterInstance(new QBitNinjaClient(_settings.CurrentValue.NinjaServiceClient.ServiceUrl))
                .AsSelf();

            builder.RegisterHistoryOperationPublisher(_settings.CurrentValue.PayHistoryServicePublisher, _log);

            builder.RegisterInvoiceConfirmationPublisher(_settings.CurrentValue.PayInvoiceConfirmationPublisher, _log);
        }

        private void RegisterCaches(ContainerBuilder builder)
        {
            builder.Register(x =>
            {
                var assetsService = x.Resolve<IComponentContext>().Resolve<IAssetsService>();

                return new CachedDataDictionary<string, Asset>
                (
                    async () =>
                    {
                        IList<Asset> assets = await Policy
                            .Handle<Exception>()
                            .WaitAndRetryAsync(
                                _settings.CurrentValue.PayInternalService.RetryPolicy.DefaultAttempts,
                                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                                (ex, timestamp) => _log.WriteError("Getting assets dictionary with retry", ex))
                            .ExecuteAsync(() => assetsService.AssetGetAllAsync(true));

                        return assets.ToDictionary(itm => itm.Id);
                    }, _settings.CurrentValue.PayInternalService.ExpirationPeriods.AssetsCache);
            }).SingleInstance();

            builder.Register(x =>
            {
                var assetsService = x.Resolve<IComponentContext>().Resolve<IAssetsService>();

                return new CachedDataDictionary<string, AssetPair>(
                    async () =>
                    {
                        IList<AssetPair> assetPairs = await Policy
                            .Handle<Exception>()
                            .WaitAndRetryAsync(
                                _settings.CurrentValue.PayInternalService.RetryPolicy.DefaultAttempts,
                                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                                (ex, timestamp) => _log.WriteError("Getting asset pairs dictionary with retry", ex))
                            .ExecuteAsync(() => assetsService.AssetPairGetAllAsync());

                        return assetPairs.ToDictionary(itm => itm.Id);
                    }, _settings.CurrentValue.PayInternalService.ExpirationPeriods.AssetsCache);
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
            builder.RegisterType<PaymentTxUrlValueResolver>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<RefundTxUrlValueResolver>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<PaymentRequestBcnWalletAddressValueResolver>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<RefundAmountResolver>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<PaymentTxBcnWalletAddressValueResolver>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<VirtualAddressResolver>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<AssetIdValueResolver>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<AssetDisplayIdValueResolver>()
                .AsSelf()
                .SingleInstance();
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<PaymentRequestExpiraitonHandler>()
                .As<IPaymentRequestExpirationHandler>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.JobPeriods.PaymentRequestExpirationHandling))
                .SingleInstance();
        }
    }
}
