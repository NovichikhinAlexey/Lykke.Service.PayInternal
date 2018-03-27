using System;
using AutoMapper;
using Common;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Mapping
{
    public class PaymentTxUrlValueResolver : IValueResolver<IPaymentRequestTransaction, object, string>
    {
        private readonly BlockchainExplorerSettings _blockchainExplorerSettings;

        public PaymentTxUrlValueResolver(BlockchainExplorerSettings blockchainExplorerSettings)
        {
            _blockchainExplorerSettings = blockchainExplorerSettings ??
                                          throw new ArgumentNullException(nameof(blockchainExplorerSettings));
        }

        public string Resolve(IPaymentRequestTransaction source, object destination, string destMember,
            ResolutionContext context)
        {
            var uri = new Uri(new Uri(_blockchainExplorerSettings.TransactionUrl.AddLastSymbolIfNotExists('/')), source.Id);

            return uri.ToString();
        }
    }
}
