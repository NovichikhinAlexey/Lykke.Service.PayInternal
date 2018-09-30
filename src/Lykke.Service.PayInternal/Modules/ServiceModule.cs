using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Lykke.Bitcoin.Api.Client;
using Lykke.Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.BlockchainWallets.Client;
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
using Lykke.Service.PayMerchant.Client;
using Lykke.Service.PayTransferValidation.Client;
using Lykke.Service.PayVolatility.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using QBitNinja.Client;

namespace Lykke.Service.PayInternal.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterSettings(builder);

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
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.RetryPolicy))
                .SingleInstance();

            builder.RegisterType<LykkeAssetsResolver>()
                .As<ILykkeAssetsResolver>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.AssetsMap));

            builder.RegisterType<MarkupService>()
                .As<IMarkupService>()
                .WithParameter("volatilityAssetPairs", _settings.CurrentValue.PayVolatilityServiceClient.AssetPairs);

            builder.RegisterType<BcnSettingsResolver>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.Blockchain))
                .As<IBcnSettingsResolver>();

            builder.RegisterType<AutoSettleSettingsResolver>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.AutoSettle))
                .As<IAutoSettleSettingsResolver>();

            builder.RegisterType<FileService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.Merchant))
                .As<IFileService>();

            builder.RegisterType<WalletHistoryService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.RetryPolicy))
                .As<IWalletHistoryService>();

            builder.RegisterType<WalletBalanceValidator>()
                .As<IWalletBalanceValidator>();

            builder.RegisterType<ConfirmationsService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.RetryPolicy))
                .As<IConfirmationsService>();

            builder.RegisterType<DepositValidationService>()
                .As<IDepositValidationService>();
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

            builder.RegisterHistoryOperationPublisher(_settings.CurrentValue.PayHistoryServicePublisher);

            builder.RegisterPayHistoryClient(_settings.CurrentValue.PayHistoryServiceClient.ServiceUrl);

            builder.RegisterInvoiceConfirmationPublisher(_settings.CurrentValue.PayInvoiceConfirmationPublisher);

            builder.RegisterCachedPayVolatilityClient(_settings.CurrentValue.PayVolatilityServiceClient, null);

            builder.RegisterPayMerchantClient(_settings.CurrentValue.PayMerchantServiceClient, null);

            builder.Register(ctx =>
                    new BlockchainWalletsClient(
                        _settings.CurrentValue.BlockchainWalletsServiceClient.ServiceUrl,
                        ctx.Resolve<ILogFactory>()))
                .As<IBlockchainWalletsClient>()
                .SingleInstance();

            builder.RegisterPayTransferValidationClient(_settings.CurrentValue.PayTransferValidationServiceClient, null);
        }

        private void RegisterCaches(ContainerBuilder builder)
        {
            builder.RegisterType<AssetsLocalCache>()
                .As<IAssetsLocalCache>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.ExpirationPeriods))
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.RetryPolicy));
        }

        private void RegisterRabbitMqPublishers(ContainerBuilder builder)
        {
            builder.RegisterType<WalletEventsPublisher>()
                .As<IWalletEventsPublisher>()
                .As<IStopable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.Rabbit));

            builder.RegisterType<PaymentRequestPublisher>()
                .As<IPaymentRequestPublisher>()
                .As<IStopable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.Rabbit));

            builder.RegisterType<TransactionPublisher>()
                .As<ITransactionPublisher>()
                .As<IStopable>()
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

        private void RegisterSettings(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings.CurrentValue.PayInternalService.Blockchain.Ethereum)
                .AsSelf()
                .SingleInstance();
        }
    }
}
