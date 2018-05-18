using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
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
        private readonly ILog _log;
        private readonly AppSettings _appSettings;
        private readonly IPaymentRequestExpirationHandler _paymentRequestExpirationHandler;

        public StartupManager(
            ILog log,
            AppSettings appSettings,
            IPaymentRequestExpirationHandler paymentRequestExpirationHandler)
        {
            _log = log?.CreateComponentScope(nameof(StartupManager)) ?? throw new ArgumentNullException(nameof(log));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _paymentRequestExpirationHandler = paymentRequestExpirationHandler ??
                                               throw new ArgumentNullException(nameof(paymentRequestExpirationHandler));
        }

        public async Task StartAsync()
        {
            await _log.WriteInfoAsync(nameof(StartAsync), string.Empty, "Checking app settings consistency...");

            TimeSpan primaryExpPeriod = _appSettings.PayInternalService.ExpirationPeriods.Order.Primary;

            TimeSpan extendedExpPeriod = _appSettings.PayInternalService.ExpirationPeriods.Order.Extended;

            if (primaryExpPeriod > extendedExpPeriod)
                throw new OrderExpirationSettingsInconsistentException(primaryExpPeriod, extendedExpPeriod);

            await _log.WriteInfoAsync(nameof(StartAsync), string.Empty, "Settings checked successfully.");

            await _log.WriteInfoAsync(nameof(StartAsync), string.Empty,
                "Starting payment request expiration handler ...");

            _paymentRequestExpirationHandler.Start();

            await _log.WriteInfoAsync(nameof(StartAsync), string.Empty,
                "Payment request expiration handler successfully started.");
        }
    }
}
