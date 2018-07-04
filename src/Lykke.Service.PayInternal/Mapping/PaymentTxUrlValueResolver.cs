using System;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Mapping
{
    public class PaymentTxUrlValueResolver : IValueResolver<IPaymentRequestTransaction, object, string>
    {
        private readonly IBcnSettingsResolver _bcnSettingsResolver;

        public PaymentTxUrlValueResolver([NotNull] IBcnSettingsResolver bcnSettingsResolver)
        {
            _bcnSettingsResolver = bcnSettingsResolver ?? throw new ArgumentNullException(nameof(bcnSettingsResolver));
        }

        public string Resolve(IPaymentRequestTransaction source, object destination, string destMember,
            ResolutionContext context)
        {
            return _bcnSettingsResolver.GetExplorerUrl(source.Blockchain, source.TransactionId);
        }
    }
}
