using System;
using Autofac.Features.Indexed;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class BlockchainClientProvider : IBlockchainClientProvider
    {
        private readonly IIndex<BlockchainType, IBlockchainApiClient> _blockchainClients;

        public BlockchainClientProvider(IIndex<BlockchainType, IBlockchainApiClient> blockchainClients)
        {
            _blockchainClients = blockchainClients;
        }

        public IBlockchainApiClient Get(BlockchainType blockchainType)
        {
            if (!_blockchainClients.TryGetValue(blockchainType, out var client))
            {
                throw new InvalidOperationException($"Blockchain client of type [{blockchainType.ToString()}] not found");
            }

            return client;
        }
    }
}
