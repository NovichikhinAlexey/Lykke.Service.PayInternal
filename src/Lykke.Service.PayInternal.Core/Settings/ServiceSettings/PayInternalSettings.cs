using System;

namespace Lykke.Service.PayInternal.Core.Settings.ServiceSettings
{
    public class PayInternalSettings
    {
        public DbSettings Db { get; set; }
        public string DataEncriptionPassword { get; set; }
        public TimeSpan OrderExpiration { get; set; }
    }
}
