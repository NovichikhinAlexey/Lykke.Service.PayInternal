using AutoMapper;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services.Mapping
{
    public class VirtualAddressResolver : IValueResolver<IWalletAddressHolder, object, string>
    {
        private readonly IBcnWalletUsageService _bcnWalletUsageService;

        public VirtualAddressResolver(IBcnWalletUsageService bcnWalletUsageService)
        {
            _bcnWalletUsageService = bcnWalletUsageService;
        }

        public string Resolve(IWalletAddressHolder source, object destination, string destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.WalletAddress)) return string.Empty;

            string virtualAddress = _bcnWalletUsageService.ResolveOccupierAsync(source.WalletAddress, source.Blockchain)
                .GetAwaiter().GetResult();

            if (string.IsNullOrEmpty(virtualAddress))
                throw new BlockchainWalletNotLinkedException(source.WalletAddress, source.Blockchain);
            
            return virtualAddress;
        }
    }
}
