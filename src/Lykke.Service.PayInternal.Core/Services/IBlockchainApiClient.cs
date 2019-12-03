﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IBlockchainApiClient
    {
        Task<BlockchainTransferResult> TransferAsync(BlockchainTransferCommand transfer);

        Task<string> CreateAddressAsync();

        Task<bool> ValidateAddressAsync(string address);

        Task<IReadOnlyList<BlockchainBalanceResult>> GetBalancesAsync(string address);

        void SetBalance(string address, decimal balance);
    }
}
