using System;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Mapping
{
    public class RefundTxUrlValueResolver : IValueResolver<PaymentRequestRefundTransaction, object, string>
    {
        private readonly IBcnExplorerResolver _bcnExplorerResolver;

        public RefundTxUrlValueResolver([NotNull] IBcnExplorerResolver bcnExplorerResolver)
        {
            _bcnExplorerResolver = bcnExplorerResolver ?? throw new ArgumentNullException(nameof(bcnExplorerResolver));
        }

        public string Resolve(PaymentRequestRefundTransaction source, object destination, string destMember,
            ResolutionContext context)
        {
            return _bcnExplorerResolver.GetExplorerUrl(source.Blockchain, source.Hash);
        }
    }
}
