using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PayInternal.Core.Settings.ServiceSettings
{
    [UsedImplicitly]
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
        [AzureTableCheck]
        public string MerchantOrderConnString { get; set; }
        [AzureTableCheck]
        public string MerchantConnString { get; set; }
        [AzureTableCheck]
        public string PaymentRequestConnString { get; set; }
        [AzureTableCheck]
        public string TransferConnString { get; set; }
    }
}
