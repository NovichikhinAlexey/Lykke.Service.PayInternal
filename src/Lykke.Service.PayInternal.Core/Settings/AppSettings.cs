using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Lykke.Service.PayInternal.Core.Settings.SlackNotifications;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PayInternal.Core.Settings
{
    public class AppSettings
    {
        public PayInternalSettings PayInternalService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public BitcoinCoreSettings BitcoinCore { get; set; }
    }

    public class BitcoinCoreSettings
    {
        [HttpCheck("api/isalive")]
        public string BitcoinCoreApiUrl { get; set; }
    }
}
