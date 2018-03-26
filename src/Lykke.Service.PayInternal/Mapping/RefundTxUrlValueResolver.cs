using System;
using AutoMapper;
using Common;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Mapping
{
    public class RefundTxUrlValueResolver : IValueResolver<PaymentRequestRefundTransaction, object, string>
    {
        private readonly BlockchainExplorerSettings _blockchainExplorerSettings;

        public RefundTxUrlValueResolver(BlockchainExplorerSettings blockchainExplorerSettings)
        {
            _blockchainExplorerSettings = blockchainExplorerSettings ??
                                          throw new ArgumentNullException(nameof(blockchainExplorerSettings));
        }

        public string Resolve(PaymentRequestRefundTransaction source, object destination, string destMember,
            ResolutionContext context)
        {
            var uri = new Uri(new Uri(_blockchainExplorerSettings.TransactionUrl.AddLastSymbolIfNotExists('/')), source.Hash);

            return uri.ToString();
        }
    }
}
