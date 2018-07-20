using System;
using System.Threading.Tasks;
using Common;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.PeriodicalHandlers
{
    public class PaymentRequestExpiraitonHandler : TimerPeriod, IPaymentRequestExpirationHandler
    {
        private readonly IPaymentRequestService _paymentRequestService;

        public PaymentRequestExpiraitonHandler(
            TimeSpan period, 
            ILogFactory logFactory, 
            IPaymentRequestService paymentRequestService) : base(period, logFactory)
        {
            _paymentRequestService = paymentRequestService;
        }

        public override async Task Execute()
        {
            await _paymentRequestService.HandleExpiredAsync();
        }
    }
}
