using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    /// <summary>
    /// Transfers amount between adresses within one blockchain
    /// 
    /// Responsibility: Decides which blockchain to use depending on assetId, registers transfer operation in repository
    /// </summary>
    public class TransferService : ITransferService
    {
        private readonly IIndex<BlockchainType, IBlockchainApiClient> _blockchainClients;
        private readonly ITransferRepository _transferRepository;

        public TransferService(
            IIndex<BlockchainType, IBlockchainApiClient> blockchainClients,
            ITransferRepository transferRepository)
        {
            _blockchainClients = blockchainClients;
            _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
        }

        public async Task<TransferResult> ExecuteAsync(TransferCommand transferCommand)
        {
            BlockchainType blockchainType;

            switch (transferCommand.AssetId)
            {
                case LykkeConstants.BitcoinAssetId:
                case LykkeConstants.SatoshiAssetId:
                    blockchainType = BlockchainType.Bitcoin;
                    break;
                default: throw new AssetNotSupportedException(transferCommand.AssetId);
            }

            if (!_blockchainClients.TryGetValue(blockchainType, out IBlockchainApiClient blockchainClient))
            {
                throw new InvalidOperationException($"Blockchain client of type [{blockchainType}] not found");
            }

            BlockchainTransferResult blockchainTransferResult =
                await blockchainClient.TransferAsync(transferCommand.ToBlockchainTransfer());

            ITransfer transfer = await _transferRepository.AddAsync(new Transfer
            {
                AssetId = transferCommand.AssetId,
                Blockchain = blockchainTransferResult.Blockchain,
                CreatedOn = DateTime.UtcNow,
                Amounts = transferCommand.Amounts,
                Transactions = blockchainTransferResult.Transactions.Select(x => x.ToTransfer())
            });

            return transfer.ToResult();
        }
    }
}
