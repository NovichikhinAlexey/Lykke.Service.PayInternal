﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bitcoin.Api.Client.AutoGenerated.Models;
using Lykke.Bitcoin.Api.Client.BitcoinApi;
using Lykke.Bitcoin.Api.Client.BitcoinApi.Models;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class BitcoinApiClient : IBlockchainApiClient
    {
        private readonly IBitcoinApiClient _bitcoinServiceClient;

        private readonly IFeeProvider _feeProvider;

        private readonly ILog _log;

        public BitcoinApiClient(
            IBitcoinApiClient bitcoinServiceClient,
            IFeeProvider feeProvider,
            ILog log)
        {
            _bitcoinServiceClient =
                bitcoinServiceClient ?? throw new ArgumentNullException(nameof(bitcoinServiceClient));
            _feeProvider = feeProvider ?? throw new ArgumentNullException(nameof(feeProvider));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<BlockchainTransferResult> TransferAsync(BlockchainTransferCommand transfer)
        {
            BlockchainTransferResult result = new BlockchainTransferResult {Blockchain = BlockchainType.Bitcoin};

            foreach (var transferAmountGroup in transfer.Amounts.GroupBy(x => x.Destination))
            {
                string destination = transferAmountGroup.Key;

                var sources = transferAmountGroup.Select(x =>
                {
                    switch (transfer.AssetId)
                    {
                        case LykkeConstants.BitcoinAssetId: 
                            return new ToOneAddress(x.Source, x.Amount);
                        case LykkeConstants.SatoshiAssetId:
                            return new ToOneAddress(x.Source, x.Amount.SatoshiToBtc());
                        default: 
                            throw new AssetNotSupportedException(transfer.AssetId);
                    }
                }).ToList();

                OnchainResponse response = await _bitcoinServiceClient.TransactionMultipleTransfer(
                    Guid.NewGuid(),
                    destination,
                    LykkeConstants.BitcoinAssetId,
                    _feeProvider.FeeRate,
                    _feeProvider.FixedFee,
                    sources);

                var errorMessage = response.HasError
                    ? $"Error placing MultipleTransfer transaction to destination address = {transferAmountGroup.Key}, code = {response.Error?.Code}, message = {response.Error?.Message}"
                    : string.Empty;

                result.Transactions.Add(new BlockchainTransactionResult
                {
                    Amount = sources.Sum(x => x.Amount ?? 0),
                    AssetId = LykkeConstants.BitcoinAssetId,
                    Hash = response.Transaction?.Hash,
                    Sources = sources.Select(x => x.Address),
                    Destinations = new List<string> {destination},
                    Error = errorMessage
                });
            }

            return result;
        }

        public async Task<string> CreateAddress()
        {
            LykkePayWallet wallet = await _bitcoinServiceClient.GenerateLykkePayWallet();

            if (wallet.HasError)
            {
                await _log.WriteWarningAsync(nameof(BitcoinApiClient), nameof(CreateAddress), wallet.Error?.Message);

                throw new WalletAddressAllocationException(BlockchainType.Bitcoin);
            }

            return wallet.Address;
        }
    }
}
