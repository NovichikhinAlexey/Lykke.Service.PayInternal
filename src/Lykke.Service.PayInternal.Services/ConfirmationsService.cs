using System;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayCallback.Client.InvoiceConfirmation;
using Lykke.Service.PayInternal.Core.Domain.Confirmations;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Polly;
using Polly.Retry;

namespace Lykke.Service.PayInternal.Services
{
    public class ConfirmationsService : IConfirmationsService
    {
        private readonly InvoiceConfirmationPublisher _invoiceConfirmationPublisher;
        private readonly RetryPolicy _retryPolicy;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly RetryPolicySettings _retryPolicySettings;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ILog _log;

        public ConfirmationsService(
            [NotNull] InvoiceConfirmationPublisher invoiceConfirmationPublisher,
            [NotNull] RetryPolicySettings retryPolicySettings, 
            [NotNull] ILog log)
        {
            _invoiceConfirmationPublisher = invoiceConfirmationPublisher ?? throw new ArgumentNullException(nameof(invoiceConfirmationPublisher));
            _retryPolicySettings = retryPolicySettings ?? throw new ArgumentNullException(nameof(retryPolicySettings));
            _log = log.CreateComponentScope(nameof(ConfirmationsService)) ?? throw new ArgumentNullException(nameof(log));
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    _retryPolicySettings.DefaultAttempts,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, timespan) => _log.WriteError("Publish invoice confirmation with retry", null, ex));
        }

        public async Task ConfirmCashoutAsync(CashoutConfirmationCommand cmd)
        {
            await _retryPolicy.ExecuteAsync(() =>
                _invoiceConfirmationPublisher.PublishAsync(Mapper.Map<InvoiceConfirmation>(cmd)));
        }
    }
}
