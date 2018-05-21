using System;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Mapping
{
    public class PaymentTxUrlValueResolver : IValueResolver<IPaymentRequestTransaction, object, string>
    {
        private readonly IBcnExplorerResolver _bcnExplorerResolver;

        public PaymentTxUrlValueResolver([NotNull] IBcnExplorerResolver bcnExplorerResolver)
        {
            _bcnExplorerResolver = bcnExplorerResolver ?? throw new ArgumentNullException(nameof(bcnExplorerResolver));
        }

        public string Resolve(IPaymentRequestTransaction source, object destination, string destMember,
            ResolutionContext context)
        {
            return _bcnExplorerResolver.GetExplorerUrl(source.Blockchain, source.TransactionId);
        }
    }
}
