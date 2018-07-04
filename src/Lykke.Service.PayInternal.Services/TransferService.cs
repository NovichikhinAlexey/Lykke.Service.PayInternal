using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
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
        private readonly IAssetSettingsService _assetSettingsService;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;

        public TransferService(
            [NotNull] ITransferRepository transferRepository,
            [NotNull] IBlockchainClientProvider blockchainClientProvider,
            [NotNull] IAssetSettingsService assetSettingsService, 
            [NotNull] ILykkeAssetsResolver lykkeAssetsResolver)
        {
            _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
            _blockchainClientProvider = blockchainClientProvider ?? throw new ArgumentNullException(nameof(blockchainClientProvider));
            _assetSettingsService = assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _lykkeAssetsResolver = lykkeAssetsResolver ?? throw new ArgumentNullException(nameof(lykkeAssetsResolver));
        }

        public async Task<TransferResult> ExecuteAsync(TransferCommand transferCommand)
        {
            BlockchainType blockchainType = await _assetSettingsService.GetNetworkAsync(transferCommand.AssetId);

            IBlockchainApiClient blockchainClient = _blockchainClientProvider.Get(blockchainType);

            BlockchainTransferCommand cmd = new BlockchainTransferCommand(transferCommand.AssetId);

            string lykkeAssetId = transferCommand.AssetId.IsGuid()
                ? transferCommand.AssetId
                : await _lykkeAssetsResolver.GetLykkeId(transferCommand.AssetId);

            foreach (var transferCommandAmount in transferCommand.Amounts)
            {
                decimal balance = await blockchainClient.GetBalanceAsync(transferCommandAmount.Source, lykkeAssetId);

                if (transferCommandAmount.Amount == null)
                {
                    if (balance > 0)
                    {
                        cmd.Amounts.Add(new TransferAmount
                        {
                            Amount = balance,
                            Source = transferCommandAmount.Source,
                            Destination = transferCommandAmount.Destination
                        });

                        continue;
                    }

                    throw new InsufficientFundsException(transferCommandAmount.Source, transferCommand.AssetId);
                }

                if (transferCommandAmount.Amount > balance)
                    throw new InsufficientFundsException(transferCommandAmount.Source, transferCommand.AssetId);

                cmd.Amounts.Add(transferCommandAmount);
            }

            BlockchainTransferResult blockchainTransferResult = await blockchainClient.TransferAsync(cmd);

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
