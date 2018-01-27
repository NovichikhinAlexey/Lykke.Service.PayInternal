namespace Lykke.Service.PayInternal.Core.Settings.ServiceSettings
{
    public class DbSettings
    {
        public string LogsConnString { get; set; }
        public string MerchantWalletConnString { get; set; }
        public string MerchantOrderConnString { get; set; }
        public string MerchantConnString { get; set; }
        public string PaymentRequestConnString { get; set; }
    }
}
