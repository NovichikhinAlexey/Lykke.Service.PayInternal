using System.Linq;
using AutoMapper;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Mapping
{
    public class BlockchainWalletAddressValueResolver : IValueResolver<IPaymentRequest, object, string>
    {
        private readonly IVirtualWalletService _virtualWalletService;

        public BlockchainWalletAddressValueResolver(IVirtualWalletService virtualWalletService)
        {
            _virtualWalletService = virtualWalletService;
        }

        public string Resolve(IPaymentRequest source, object destination, string destMember, ResolutionContext context)
        {
            IVirtualWallet virtualWallet = 
                _virtualWalletService.GetAsync(source.MerchantId, source.WalletAddress).GetAwaiter().GetResult();

            BlockchainWallet bcnWallet =
                virtualWallet.BlockchainWallets.SingleOrDefault(x => x.AssetId == source.PaymentAssetId);

            return bcnWallet?.Address;
        }
    }
}
