using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PayInternal.Contract.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using PaymentRequestRefund = Lykke.Service.PayInternal.Contract.PaymentRequest.PaymentRequestRefund;

namespace Lykke.Service.PayInternal.Rabbit.Publishers
{
    public class PaymentRequestPublisher : IPaymentRequestPublisher, IStartable, IStopable
    {
        private readonly IPaymentRequestDetailsBuilder _paymentRequestDetailsBuilder;
        private readonly ILog _log;
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _settings;
        private RabbitMqPublisher<PaymentRequestDetailsMessage> _publisher;

        public PaymentRequestPublisher(
            RabbitMqSettings settings, 
            IPaymentRequestDetailsBuilder paymentRequestDetailsBuilder,
            ILogFactory logFactory)
        {
            _settings = settings;
            _paymentRequestDetailsBuilder = paymentRequestDetailsBuilder;
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForPublisher(_settings.ConnectionString, _settings.PaymentRequestsExchangeName);

            settings.MakeDurable();

            _publisher = new RabbitMqPublisher<PaymentRequestDetailsMessage>(_logFactory, settings)
                .SetSerializer(new JsonMessageSerializer<PaymentRequestDetailsMessage>())
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

        public async Task PublishAsync(IPaymentRequest paymentRequest, Core.Domain.PaymentRequests.PaymentRequestRefund refundInfo)
        {
            PaymentRequestDetailsMessage message = await _paymentRequestDetailsBuilder.Build<
                    PaymentRequestDetailsMessage, 
                    PaymentRequestOrder, 
                    PaymentRequestTransaction, 
                    PaymentRequestRefund>(paymentRequest, refundInfo);

            await PublishAsync(message);
        }
        
        public async Task PublishAsync(PaymentRequestDetailsMessage message)
        {
            _log.Info("Publishing payment request status update message", message);
      
            await _publisher.ProduceAsync(message);
        }
    }
}
