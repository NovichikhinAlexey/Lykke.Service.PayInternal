using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.PeriodicalHandlers
{
    public class PaymentRequestExpiraitonHandler : TimerPeriod
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly ILog _log;

        public PaymentRequestExpiraitonHandler(
            TimeSpan period, 
            ILog log, 
            IPaymentRequestService paymentRequestService) : base(
            nameof(PaymentRequestExpiraitonHandler), (int) period.TotalMilliseconds, log)
        {
            _paymentRequestService = paymentRequestService;
            _log = log?.CreateComponentScope(nameof(PaymentRequestExpiraitonHandler)) ??
                   throw new ArgumentNullException(nameof(log));
        }

        public override async Task Execute()
        {
            await _paymentRequestService.HandleExpiredAsync();
        }
    }
}
