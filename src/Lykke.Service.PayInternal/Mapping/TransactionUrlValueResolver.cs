using System;
using AutoMapper;
using Common;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Lykke.Service.PayInternal.Models.PaymentRequests;

namespace Lykke.Service.PayInternal.Mapping
{
    public class TransactionUrlValueResolver : IValueResolver<IBlockchainTransaction, PaymentRequestTransactionModel, string>
    {
        private readonly BlockchainExplorerSettings _blockchainExplorerSettings;

        public TransactionUrlValueResolver(BlockchainExplorerSettings blockchainExplorerSettings)
        {
            _blockchainExplorerSettings = blockchainExplorerSettings ??
                                          throw new ArgumentNullException(nameof(blockchainExplorerSettings));
        }

        public string Resolve(IBlockchainTransaction source, PaymentRequestTransactionModel destination, string destMember,
            ResolutionContext context)
        {
            var uri = new Uri(new Uri(_blockchainExplorerSettings.TransactionUrl.AddLastSymbolIfNotExists('/')), source.Id);

            return uri.ToString();
        }
    }
}
