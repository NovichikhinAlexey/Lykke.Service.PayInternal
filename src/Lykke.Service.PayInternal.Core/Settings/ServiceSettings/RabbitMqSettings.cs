namespace Lykke.Service.PayInternal.Core.Settings.ServiceSettings
{
    public class RabbitMqSettings
    {
        public string ConnectionString { get; set; }
        public string WalletsExchangeName { get; set; }
        public string PaymentRequestsExchangeName { get; set; }
    }
}
