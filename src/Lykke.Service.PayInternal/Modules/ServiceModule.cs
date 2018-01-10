using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Bitcoin.Api.Client;
using Lykke.Service.PayInternal.AzureRepositories.Order;
using Lykke.Service.PayInternal.AzureRepositories.Wallet;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings;
using Lykke.Service.PayInternal.Services;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
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

            builder.Populate(_services);
        }

        private void RegisterAzureRepositories(ContainerBuilder builder)
        {
            builder.RegisterInstance<IWalletRepository>(new WalletRepository(
                AzureTableStorage<WalletEntity>.Create(_dbSettings.ConnectionString(x => x.MerchantWalletConnString),
                    "MerchantWallets", _log)));

            builder.RegisterInstance<IOrdersRepository>(new OrdersRepository(
                AzureTableStorage<OrderEntity>.Create(_dbSettings.ConnectionString(x => x.MerchantWalletConnString),
                    "MerchantOrderRequest", _log)));
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

            builder.RegisterType<CryptoService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInternalService.DataEncriptionPassword))
                .As<ICryptoService>();

            builder.RegisterType<MerchantWalletsService>()
                .As<IMerchantWalletsService>();
        }

        private void RegisterServiceClients(ContainerBuilder builder)
        {
            builder.RegisterBitcoinApiClient(_settings.CurrentValue.BitcoinCore.BitcoinCoreApiUrl);
        }
    }
}
