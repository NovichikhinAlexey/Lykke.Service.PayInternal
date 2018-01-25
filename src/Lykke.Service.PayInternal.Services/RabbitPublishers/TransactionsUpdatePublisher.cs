using System.Threading.Tasks;
using Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PayInternal.Contract;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services.RabbitPublishers
{
    public class TransactionsUpdatePublisher : ITransactionUpdatesPublisher
    {
        private readonly ILog _log;
        private readonly RabbitMqSettings _settings;
        private RabbitMqPublisher<TransactionUpdateMessage> _publisher;

        public TransactionsUpdatePublisher(ILog log, RabbitMqSettings settings)
        {
            _log = log;
            _settings = settings;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForPublisher(_settings.ConnectionString, _settings.TransactionUpdatesExchangeName);

            settings.MakeDurable();

            _publisher = new RabbitMqPublisher<TransactionUpdateMessage>(settings)
                .SetSerializer(new JsonMessageSerializer<TransactionUpdateMessage>())
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

        public async Task PublishAsync(TransactionUpdateMessage message)
        {
            await _publisher.ProduceAsync(message);
        }
    }
}
