using AutoMapper;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Mapping
{
    public class BlockchainWalletAddressValueResolver : IValueResolver<IPaymentRequest, object, string>
    {
        private readonly IWalletManager _walletManager;

        public BlockchainWalletAddressValueResolver(IWalletManager walletManager)
        {
            _walletManager = walletManager;
        }

        public string Resolve(IPaymentRequest source, object destination, string destMember, ResolutionContext context)
        {
            return _walletManager.ResolveBlockchainAddress(source.WalletAddress, source.PaymentAssetId).GetAwaiter().GetResult();
        }
    }
}
