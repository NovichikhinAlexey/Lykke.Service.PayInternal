using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    /// <summary>
    /// Transfers amount between adresses within one blockchain
    /// 
    /// Responsibility: Decides which blockchain to use depending on assetId, registers transfer operation in repository
    /// </summary>
    public class TransferService : ITransferService
    {
        private readonly IBlockchainClientProvider _blockchainClientProvider;
        private readonly ITransferRepository _transferRepository;
        private readonly IAssetsAvailabilityService _assetsAvailabilityService;

        public TransferService(
            [NotNull] ITransferRepository transferRepository,
            [NotNull] IBlockchainClientProvider blockchainClientProvider,
            [NotNull] IAssetsAvailabilityService assetsAvailabilityService)
        {
            _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
            _blockchainClientProvider = blockchainClientProvider ?? throw new ArgumentNullException(nameof(blockchainClientProvider));
            _assetsAvailabilityService = assetsAvailabilityService ?? throw new ArgumentNullException(nameof(assetsAvailabilityService));
        }

        public async Task<TransferResult> ExecuteAsync(TransferCommand transferCommand)
        {
            BlockchainType blockchainType = await _assetsAvailabilityService.GetNetworkAsync(transferCommand.AssetId);

            IBlockchainApiClient blockchainClient = _blockchainClientProvider.Get(blockchainType);

            BlockchainTransferResult blockchainTransferResult =
                await blockchainClient.TransferAsync(transferCommand.ToBlockchainTransfer());

            ITransfer transfer = await _transferRepository.AddAsync(new Transfer
            {
                AssetId = transferCommand.AssetId,
                Blockchain = blockchainTransferResult.Blockchain,
                CreatedOn = DateTime.UtcNow,
                Amounts = transferCommand.Amounts,
                Transactions = Mapper.Map<IEnumerable<TransferTransaction>>(blockchainTransferResult.Transactions)
            });

            return Mapper.Map<TransferResult>(transfer);
        }

        public async Task<Transfer> GetAsync(string id)
        {
            return await _transferRepository.GetAsync(id);
        }
    }
}
