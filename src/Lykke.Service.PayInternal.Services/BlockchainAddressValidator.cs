using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class BlockchainAddressValidator : IBlockchainAddressValidator
    {
        private readonly IBlockchainClientProvider _blockchainClientProvider;
        private readonly ILog _log;
        private static readonly IReadOnlyList<BlockchainType> SupportedNetworks = new[]
            {BlockchainType.Bitcoin, BlockchainType.Ethereum, BlockchainType.EthereumIata};

        public BlockchainAddressValidator(
            [NotNull] IBlockchainClientProvider blockchainClientProvider, 
            [NotNull] ILogFactory logFactory)
        {
            _blockchainClientProvider = blockchainClientProvider;
            _log = logFactory.CreateLog(this);
        }

        public async Task<bool> Execute(string address, BlockchainType blockchain)
        {
            if (!SupportedNetworks.Contains(blockchain))
                throw new BlockchainTypeNotSupported(blockchain);

            IBlockchainApiClient blockchainClient = _blockchainClientProvider.Get(blockchain);

            try
            {
                return await blockchainClient.ValidateAddressAsync(address);
            }
            catch (WalletAddressValidationException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    Blockchain = e.Blockchain.ToString(),
                    e.Address
                });

                throw;
            }
            catch (UnrecognizedApiResponse e)
            {
                _log.ErrorWithDetails(e, new {e.ResponseType});

                throw;
            }
        }
    }
}
