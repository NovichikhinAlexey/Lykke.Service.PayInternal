using System;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Mapping
{
    public class BilBlockchainValueResolver : IMemberValueResolver<object, object, string, BlockchainType>
    {
        private readonly BlockchainMapSettings _blockchainMapSettings;

        public BilBlockchainValueResolver([NotNull] BlockchainMapSettings blockchainMapSettings)
        {
            _blockchainMapSettings = blockchainMapSettings ?? throw new ArgumentNullException(nameof(blockchainMapSettings));
        }

        public BlockchainType Resolve(object source, object destination, string sourceMember, BlockchainType destMember,
            ResolutionContext context)
        {
            if (_blockchainMapSettings.Values.TryGetValue(sourceMember, out var blockchain))
            {
                return blockchain;
            }

            throw new BilBlockchainTypeNotSupported(sourceMember);
        }
    }
}
