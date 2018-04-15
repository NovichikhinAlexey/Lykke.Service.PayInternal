using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services.Mapping
{
    public class RefundAmountResolver : IValueResolver<IPaymentRequestTransaction, object, IEnumerable<TransferAmount>>
    {
        private readonly IWalletManager _walletManager;

        public RefundAmountResolver(IWalletManager walletManager)
        {
            _walletManager = walletManager;
        }

        public IEnumerable<TransferAmount> Resolve(IPaymentRequestTransaction source, object destination,
            IEnumerable<TransferAmount> destMember, ResolutionContext context)
        {
            string destinationAddress = context.Items["destinationAddress"].ToString();

            string bcnAddress = _walletManager.ResolveBlockchainAddressAsync(source.WalletAddress, source.AssetId)
                .GetAwaiter().GetResult();

            return new List<TransferAmount>
            {
                new TransferAmount
                {
                    Amount = source.Amount,
                    Source = bcnAddress,
                    Destination = string.IsNullOrWhiteSpace(destinationAddress)
                        ? source.SourceWalletAddresses.Single()
                        : destinationAddress
                }
            };
        }
    }
}
