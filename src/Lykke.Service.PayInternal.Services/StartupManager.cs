using System;
using Autofac;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings;

namespace Lykke.Service.PayInternal.Services
{
    // NOTE: Sometimes, startup process which is expressed explicitly is not just better, 
    // but the only way. If this is your case, use this class to manage startup.
    // For example, sometimes some state should be restored before any periodical handler will be started, 
    // or any incoming message will be processed and so on.
    // Do not forget to remove As<IStartable>() and AutoActivate() from DI registartions of services, 
    // which you want to startup explicitly.
    [UsedImplicitly]
    public class StartupManager : IStartupManager
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly AppSettings _appSettings;
        private readonly IPaymentRequestExpirationHandler _paymentRequestExpirationHandler;
        private readonly IWalletEventsPublisher _walletEventsPublisher;
        private readonly IPaymentRequestPublisher _paymentRequestPublisher;
        private readonly ITransactionPublisher _transactionPublisher;
        private readonly ILog _log;

        public StartupManager(
            [NotNull] AppSettings appSettings,
            [NotNull] IPaymentRequestExpirationHandler paymentRequestExpirationHandler,
            [NotNull] IWalletEventsPublisher walletEventsPublisher, 
            [NotNull] IPaymentRequestPublisher paymentRequestPublisher, 
            [NotNull] ITransactionPublisher transactionPublisher,
            [NotNull] ILogFactory logFactory)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _paymentRequestExpirationHandler = paymentRequestExpirationHandler ??
                                               throw new ArgumentNullException(nameof(paymentRequestExpirationHandler));
            _walletEventsPublisher = walletEventsPublisher ?? throw new ArgumentNullException(nameof(walletEventsPublisher));
            _paymentRequestPublisher = paymentRequestPublisher ?? throw new ArgumentNullException(nameof(paymentRequestPublisher));
            _transactionPublisher = transactionPublisher ?? throw new ArgumentNullException(nameof(transactionPublisher));
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            _log.Info(nameof(Start), "Checking app settings consistency...");

            TimeSpan primaryExpPeriod = _appSettings.PayInternalService.ExpirationPeriods.Order.Primary;

            TimeSpan extendedExpPeriod = _appSettings.PayInternalService.ExpirationPeriods.Order.Extended;

            if (primaryExpPeriod > extendedExpPeriod)
                throw new OrderExpirationSettingsInconsistentException(primaryExpPeriod, extendedExpPeriod);

            _log.Info(nameof(Start), "Settings checked successfully.");

            StartComponent("Payment request expiration handler", _paymentRequestExpirationHandler);

            StartComponent("Wallet events publisher", _walletEventsPublisher);

            StartComponent("Payment request publisher", _paymentRequestPublisher);

            StartComponent("Transaction publisher", _transactionPublisher);
        }

        private void StartComponent(string componentDisplayName, object component)
        {
            _log.Info(nameof(Start), $"Starting {componentDisplayName} ...");

            if (component is IStartable startableComponent)
            {
                startableComponent.Start();

                _log.Info(nameof(Start), $"{componentDisplayName} successfully started.");
            }
            else
            {
                _log.Warning(nameof(Start), "Component has not been started", context: component);
            }
        }
    }
}
