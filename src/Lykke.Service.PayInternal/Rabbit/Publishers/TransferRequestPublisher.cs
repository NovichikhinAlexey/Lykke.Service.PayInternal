using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PayInternal.Contract.TransferRequest;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Rabbit.Publishers
{
    public class TransferRequestPublisher : ITransferRequestPublisher, IStartable, IStopable
    {
        private readonly ITransferRequestService _transferRequestService;
        private readonly ILog _log;
        private readonly RabbitMqSettings _settings;
        private RabbitMqPublisher<TransferRequestsMessage> _publisher;

        public TransferRequestPublisher(
            ITransferRequestService transferRequestService,
            ILog log,
            RabbitMqSettings settings)
        {
            _transferRequestService = transferRequestService;
            _log = log;
            _settings = settings;
        }

        public async Task PublishAsync(ITransferRequest request)
        {
            var message = Mapper.Map<TransferRequestsMessage>(request);
            message.TransactionRequests = Mapper.Map<List<TransactionRequestMessage>>(request.TransactionRequests);
            foreach (var transactionRequest in message.TransactionRequests)
            {
                transactionRequest.SourceAmounts = Mapper.Map<List<SourceAmountMessage>>(transactionRequest.SourceAmounts);
            }

            await PublishAsync(message);
        }

      

        public async Task PublishAsync(TransferRequestsMessage message)
        {
            await _publisher.ProduceAsync(message);
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForPublisher(_settings.ConnectionString, _settings.TansferRequestsExchangeName);

            settings.MakeDurable();

            _publisher = new RabbitMqPublisher<TransferRequestsMessage>(settings)
                .SetSerializer(new JsonMessageSerializer<TransferRequestsMessage>())
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
    }
}
