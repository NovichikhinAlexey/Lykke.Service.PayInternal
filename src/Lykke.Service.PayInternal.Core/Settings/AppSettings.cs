using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Lykke.Service.PayInternal.Core.Settings.SlackNotifications;

namespace Lykke.Service.PayInternal.Core.Settings
{
    public class AppSettings
    {
        public PayInternalSettings PayInternalService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
