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
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public MarketProfileServiceClientSettings MarketProfileServiceClient { get; set; }
        public NinjaServiceClientSettings NinjaServiceClient { get; set; }
    }

    public class BitcoinCoreSettings
    {
        [HttpCheck("api/isalive")]
        public string BitcoinCoreApiUrl { get; set; }
    }

    public class AssetsServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }

    public class MarketProfileServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }

    public class NinjaServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
