using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class BlockchainAddressValidator : IBlockchainAddressValidator
    {
        private readonly IBlockchainClientProvider _blockchainClientProvider;
        private readonly ILog _log;

        public BlockchainAddressValidator(IBlockchainClientProvider blockchainClientProvider, ILog log)
        {
            _blockchainClientProvider = blockchainClientProvider;
            _log = log;
        }

        public async Task<bool> Execute(string address, BlockchainType blockchain)
        {
            IBlockchainApiClient blockchainClient = _blockchainClientProvider.Get(blockchain);

            try
            {
                return await blockchainClient.ValidateAddressAsync(address);
            }
            catch (WalletAddressValidationException validationEx)
            {
                await _log.WriteErrorAsync(nameof(BlockchainAddressValidator), nameof(Execute), new
                {
                    Blockchain = validationEx.Blockchain.ToString(),
                    validationEx.Address
                }.ToJson(), validationEx);

                throw;
            }
            catch (UnrecognizedApiResponse responseEx)
            {
                await _log.WriteErrorAsync(nameof(BlockchainAddressValidator), nameof(Execute), new
                {
                    responseEx.ResponseType
                }.ToJson(), responseEx);

                throw;
            }
        }
    }
}
