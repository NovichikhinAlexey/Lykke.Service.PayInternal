using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
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
        private readonly ILogFactory _logFactory;
        private readonly ILog _log;
        private readonly RabbitMqSettings _settings;
        private RabbitMqPublisher<NewWalletMessage> _publisher;

        public WalletEventsPublisher(
            [NotNull] ILogFactory logFactory, 
            [NotNull] RabbitMqSettings settings)
        {
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
            _settings = settings;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForPublisher(_settings.ConnectionString, _settings.WalletsExchangeName);

            settings.MakeDurable();

            _publisher = new RabbitMqPublisher<NewWalletMessage>(_logFactory, settings)
                .SetSerializer(new JsonMessageSerializer<NewWalletMessage>())
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                .PublishSynchronously()
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
            var message = new NewWalletMessage
            {
                Address = walletAddress,
                Blockchain = Enum.Parse<Contract.BlockchainType>(blockchain.ToString()),
                DueDate = dueDate
            };

            _log.Info("Publishing new wallet message", message);

            await PublishAsync(message);
        }
    }
}
