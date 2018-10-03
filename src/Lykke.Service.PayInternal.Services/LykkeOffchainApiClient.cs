using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class LykkeOffchainApiClient : IBlockchainApiClient
    {
        public async Task<BlockchainTransferResult> TransferAsync(BlockchainTransferCommand transfer)
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> CreateAddressAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> DeleteAddressAsync(string address)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> ValidateAddressAsync(string address)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IReadOnlyList<BlockchainBalanceResult>> GetBalancesAsync(string address)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IReadOnlyList<BlockchainBalanceResult>> GetBalanceAsync(string address)
        {
            throw new System.NotImplementedException();
        }
    }
}
