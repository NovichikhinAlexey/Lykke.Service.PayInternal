using System;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services.Mapping
{
    public class VirtualAddressResolver : IMemberValueResolver<IBlockchainTypeHolder, object, string, string>
    {
        private readonly IBcnWalletUsageService _bcnWalletUsageService;

        public VirtualAddressResolver(
            [NotNull] IBcnWalletUsageService bcnWalletUsageService)
        {
            _bcnWalletUsageService =
                bcnWalletUsageService ?? throw new ArgumentNullException(nameof(bcnWalletUsageService));
        }

        public string Resolve(IBlockchainTypeHolder source, object destination, string sourceMember, string destMember,
            ResolutionContext context)
        {
            if (string.IsNullOrEmpty(sourceMember)) return string.Empty;

            string virtualAddress = _bcnWalletUsageService.ResolveOccupierAsync(sourceMember, source.Blockchain)
                .GetAwaiter().GetResult();

            if (string.IsNullOrEmpty(virtualAddress))
                throw new BlockchainWalletNotLinkedException(sourceMember, source.Blockchain);

            return virtualAddress;
        }
    }
}
