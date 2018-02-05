using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PayInternal.Contract.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Rabbit.Publishers
{
    public class PaymentRequestPublisher : IPaymentRequestPublisher, IStartable, IStopable
    {
        private readonly IOrderService _orderService;
        private readonly ITransactionsService _transactionsService;
        private readonly ILog _log;
        private readonly RabbitMqSettings _settings;
        private RabbitMqPublisher<PaymentRequestDetailsMessage> _publisher;

        public PaymentRequestPublisher(
            IOrderService orderService,
            ITransactionsService transactionsService,
            ILog log,
            RabbitMqSettings settings)
        {
            _orderService = orderService;
            _transactionsService = transactionsService;
            _log = log;
            _settings = settings;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForPublisher(_settings.ConnectionString, _settings.PaymentRequestsExchangeName);

            settings.MakeDurable();

            _publisher = new RabbitMqPublisher<PaymentRequestDetailsMessage>(settings)
                .SetSerializer(new JsonMessageSerializer<PaymentRequestDetailsMessage>())
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

        public async Task PublishAsync(IPaymentRequest paymentRequest)
        {
            IOrder order = await _orderService.GetAsync(paymentRequest.Id, paymentRequest.OrderId);

            IReadOnlyList<IBlockchainTransaction> transactions =
                (await _transactionsService.GetAsync(paymentRequest.WalletAddress)).ToList();

            var message = Mapper.Map<PaymentRequestDetailsMessage>(paymentRequest);
            message.Order = Mapper.Map<PaymentRequestOrder>(order);
            message.Transactions = Mapper.Map<List<PaymentRequestTransaction>>(transactions);

            await PublishAsync(message);
        }
        
        public async Task PublishAsync(PaymentRequestDetailsMessage message)
        {
            await _publisher.ProduceAsync(message);
        }
    }
}
