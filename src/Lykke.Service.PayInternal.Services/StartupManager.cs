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

        public StartupManager(
            ILog log,
            AppSettings appSettings)
        {
            _log = log;
            _appSettings = appSettings;
        }

        public async Task StartAsync()
        {
            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "Checking app settings consistency...");

            TimeSpan primaryExpPeriod = _appSettings.PayInternalService.ExpirationPeriods.Order.Primary;

            TimeSpan extendedExpPeriod = _appSettings.PayInternalService.ExpirationPeriods.Order.Extended;

            if (primaryExpPeriod > extendedExpPeriod)
                throw new OrderExpirationSettingsInconsistentException(primaryExpPeriod, extendedExpPeriod);

            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "Settings checked successfully.");
        }
    }
}
