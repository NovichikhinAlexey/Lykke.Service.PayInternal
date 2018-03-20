using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PayInternal.Core.Settings.ServiceSettings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        public string WalletsExchangeName { get; set; }
        public string PaymentRequestsExchangeName { get; set; }
        public string TransactionUpdatesExchangeName { get; set; }
    }
}
