using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PayInternal.Contract;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using BlockchainType = Lykke.Service.PayInternal.Core.BlockchainType;

namespace Lykke.Service.PayInternal.Rabbit.Publishers
{
    public class WalletEventsPublisher : IWalletEventsPublisher, IStartable, IStopable
    {
        private readonly ILog _log;
        private readonly RabbitMqSettings _settings;
        private RabbitMqPublisher<NewWalletMessage> _publisher;

        public WalletEventsPublisher(ILog log, RabbitMqSettings settings)
        {
            _log = log;
            _settings = settings;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForPublisher(_settings.ConnectionString, _settings.WalletsExchangeName);

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

        public async Task PublishAsync(string walletAddress, BlockchainType blockchain, DateTime dueDate)
        {
            await PublishAsync(new NewWalletMessage
            {
                Address = walletAddress,
                Blockchain = Enum.Parse<Contract.BlockchainType>(blockchain.ToString()),
                DueDate = dueDate
            });
        }
    }
}
