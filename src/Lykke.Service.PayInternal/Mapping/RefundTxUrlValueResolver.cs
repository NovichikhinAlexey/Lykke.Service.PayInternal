using System;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Mapping
{
    public class RefundTxUrlValueResolver : IValueResolver<PaymentRequestRefundTransaction, object, string>
    {
        private readonly IBcnSettingsResolver _bcnSettingsResolver;

        public RefundTxUrlValueResolver([NotNull] IBcnSettingsResolver bcnSettingsResolver)
        {
            _bcnSettingsResolver = bcnSettingsResolver ?? throw new ArgumentNullException(nameof(bcnSettingsResolver));
        }

        public string Resolve(PaymentRequestRefundTransaction source, object destination, string destMember,
            ResolutionContext context)
        {
            return _bcnSettingsResolver.GetExplorerUrl(source.Blockchain, source.Hash);
        }
    }
}
