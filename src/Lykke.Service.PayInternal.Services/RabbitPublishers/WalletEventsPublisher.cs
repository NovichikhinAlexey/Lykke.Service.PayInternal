using System.Threading.Tasks;
using Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PayInternal.Contract;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services.RabbitPublishers
{
    public class WalletEventsPublisher : IWalletEventsPublisher
    {
        private readonly ILog _log;
        private readonly string _connectionString;
        private RabbitMqPublisher<NewWalletMessage> _publisher;

        public WalletEventsPublisher(ILog log, string connectionString)
        {
            _log = log;
            _connectionString = connectionString;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForPublisher(_connectionString, "pay.wallets");

            settings.MakeDurable();

            _publisher = new RabbitMqPublisher<NewWalletMessage>(settings)
                .SetSerializer(new JsonMessageSerializer<NewWalletMessage>())
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                .PublishSynchronously()
                .SetLogger(_log)
                .Start();
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }

        public void Stop()
        {
            _publisher?.Stop();
        }

        public async Task PublishAsync(NewWalletMessage message)
        {
            await _publisher.ProduceAsync(message);
        }
    }
}
