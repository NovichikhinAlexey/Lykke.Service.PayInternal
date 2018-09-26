using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PaySettlement.Contracts.Events;
using System.Threading.Tasks;
using AutoMapper;

namespace Lykke.Service.PayInternal.Cqrs.Projection
{
    public class SettlementProjection
    {
        private readonly IPaymentRequestService _paymentRequestService;

        public SettlementProjection(IPaymentRequestService paymentRequestService)
        {
            _paymentRequestService = paymentRequestService;
        }

        public Task Handle(SettlementTransferToMarketQueuedEvent e, string boundedContext)
        {
            return _paymentRequestService.UpdateStatusAsync(e.MerchantId, e.PaymentRequestId,
                new PaymentRequestStatusInfo()
                {
                    Status = PaymentRequestStatus.SettlementInProgress
                });
        }

        public Task Handle(SettlementTransferredToMerchantEvent e, string boundedContext)
        {
            return _paymentRequestService.UpdateStatusAsync(e.MerchantId, e.PaymentRequestId,
                new PaymentRequestStatusInfo()
                {
                    Status = PaymentRequestStatus.Settled
                });
        }

        public Task Handle(SettlementErrorEvent e, string boundedContext)
        {
            return _paymentRequestService.UpdateStatusAsync(e.MerchantId, e.PaymentRequestId,
                new PaymentRequestStatusInfo()
                {
                    Status = PaymentRequestStatus.SettlementError,
                    ProcessingError = Mapper.Map<PaymentRequestProcessingError>(e.Error)
                });
        }
    }
}
