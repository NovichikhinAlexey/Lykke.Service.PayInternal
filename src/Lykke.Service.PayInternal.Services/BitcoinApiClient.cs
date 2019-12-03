﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Bitcoin.Api.Client.AutoGenerated.Models;
using Lykke.Bitcoin.Api.Client.BitcoinApi;
using Lykke.Bitcoin.Api.Client.BitcoinApi.Models;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using NBitcoin;

namespace Lykke.Service.PayInternal.Services
{
    public class BitcoinApiClient : IBlockchainApiClient
    {
        private readonly IBitcoinApiClient _bitcoinServiceClient;
        private readonly IFeeProvider _feeProvider;
        private readonly Network _bitcoinNetwork;
        private readonly ILog _log;

        public BitcoinApiClient(
            [NotNull] IBitcoinApiClient bitcoinServiceClient,
            [NotNull] IFeeProvider feeProvider,
            [NotNull] ILogFactory logFactory,
            [NotNull] string bitcoinNetwork)
        {
            _bitcoinServiceClient =
                bitcoinServiceClient ?? throw new ArgumentNullException(nameof(bitcoinServiceClient));
            _feeProvider = feeProvider ?? throw new ArgumentNullException(nameof(feeProvider));
            _log = logFactory.CreateLog(this);
            _bitcoinNetwork = Network.GetNetwork(bitcoinNetwork);
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
                        case LykkeConstants.BitcoinAsset: 
                            return new ToOneAddress(x.Source, x.Amount);
                        case LykkeConstants.SatoshiAsset:
                            return new ToOneAddress(x.Source, x.Amount?.SatoshiToBtc());
                        default: 
                            throw new AssetNotSupportedException(transfer.AssetId);
                    }
                }).ToList();

                OnchainResponse response = await _bitcoinServiceClient.TransactionMultipleTransfer(
                    Guid.NewGuid(),
                    destination,
                    LykkeConstants.BitcoinAsset,
                    _feeProvider.FeeRate,
                    _feeProvider.FixedFee,
                    sources);

                var errorMessage = response.HasError
                    ? $"Error placing MultipleTransfer transaction to destination address = {transferAmountGroup.Key}, code = {response.Error?.Code}, message = {response.Error?.Message}"
                    : string.Empty;

                result.Transactions.Add(new BlockchainTransactionResult
                {
                    Amount = sources.Sum(x => x.Amount ?? 0),
                    AssetId = LykkeConstants.BitcoinAsset,
                    Hash = response.Transaction?.Hash,
                    IdentityType = TransactionIdentityType.Hash,
                    Identity = response.Transaction?.Hash,
                    Sources = sources.Select(x => x.Address),
                    Destinations = new List<string> {destination},
                    Error = errorMessage
                });
            }

            return result;
        }

        private Random _rnd = new Random();
        private string[] _defaultAddresses = new[]
        {
            "175gqn8YxbLtjQS4PXu87EpY1MSB5VG3uC",
            "16cPY8chW1NfN4PjeK9gowKDWhvGDEBCAB",
            "1BhWX6yZMbcUjXAg2uS5fb2A1GNpdYNj3R",
            "1vHpLWuscX8KrNaNzRDXhjpWBeBGpaz74",
            "1JFZA5jDQPonG82DXGn21M44bhuoMN2rjL",
            "1MBbQS4YYYLDhF81xwTioUfPtKikmw4PpZ",
            "1CvKt5ExjPBJXxHWU4Z8NC2H4oKzQX2jCt",
            "1QDsdRo7Jc2MeJxWWZQ8TdhDahULEQDgXc",
            "17pzeLgu3Ccm5urq7U6Q2p2j7KfHNpU5kh",
            "1QA8JhYnfYiFQHQMaLWUCCx4bxahLjB85J",
            "13GVUxjzkVMy1fjARJN8RqXjUiTgrYtJEc",
            "1N8gz6gPL1Y4zD3hu1HFmqXsJ7PhZTTyom",
            "1KvivaoogUFE9V4ZPfKuHT2htovzQsjUac",
            "1F5YWDMYLHzxpMjyKduWVXiLrJDvgBWoud",
            "1N5EH5aXrFwwCfgitK1xDwQoaeS8WdBh1n",
            "1JKc9aHRHyPibBe7gMpFVzSNu4JKogMGEt",
            "17Fu4nBzQM7oTsAJMRHNwNKBt2qQF9ne1R",
            "1FpccK9ThRKLYv3e6pyMRHQcTK1VtFWvcj",
            "112zuSzHMpmyRJudtL7Zw3UVc69whmHBWB",
            "1QG2RxxC74m1grHxqGxRqv9UxawrrUhTuH",
            "15cyFMjrEUcrBY1ebq9vj6TxtkXe5AuKL9",
            "13JQNHjKePkQp7H1VadFEkUaLeSu64WdY2",
            "1sJzHm4n6PgCGLNe23bboVXAEreoNieiL",
            "1LdkGjgiCV8Sn8RMAbjeEY18K8LFBcUUWh",
            "1JxFTXX7bsMXHNUtvxuFvapneVuk3hVect",
            "1C7AbagBVay39VfgMV6oL9y8YXuQEJCcHm",
            "1MBcmEPd9vMDXckPG1RMyavLQaV35SY5jb",
            "19vssBb5TuEHYvTTEbUqDhb5fmwh617KH4",
            "12eeAXnhoUpncWJy1M6veyFCTdWRR1PtGQ",
            "1JS1RJUjngkDecFnGewKYHzqnUyYZUvogg",
            "12GdAoZbmg5U5pWSQQDSPsqq2EgLttjjX1",
            "1JdwvuhfX6gotEzsGoNWJqVN9vzMeL8jTR",
            "1ACQ72BNphGvuw5ctQhM55pcEmF5QXUCQv",
            "1GGaVHgciT81oS9XSMz1xVQSWbbyQyBLdq",
            "1GAVbCTQeFcahNPpjNgBGuUzqiGi1e8J3y",
        };

        private Dictionary<string, decimal> _balances = new Dictionary<string, decimal>();

        public void SetBalance(string address, decimal balance)
        {
            _balances[address] = balance;
        }

        public void ResetBalance()
        {
            _balances.Clear();
        }

        public async Task<string> CreateAddressAsync()
        {
            var list = _defaultAddresses.Where(a => !_balances.ContainsKey(a)).ToArray();
            if (!list.Any())
            {
                ResetBalance();
                list = _defaultAddresses;
            }
            return list[_rnd.Next(list.Length-1)];
        }

        public Task<bool> ValidateAddressAsync(string address)
        {
            try
            {
                _bitcoinNetwork.Parse(address);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public async Task<IReadOnlyList<BlockchainBalanceResult>> GetBalancesAsync(string address)
        {
            var balanceResult = new BlockchainBalanceResult
            {
                AssetId = LykkeConstants.BitcoinAsset,
                Balance = 0
            };

            try
            {
                if (_balances.TryGetValue(address, out var balance))
                    balanceResult.Balance = balance;
                else
                    balanceResult.Balance = 0;
            }
            catch (Exception e)
            {
                _log.ErrorWithDetails(e, new {address});
            }

            return new List<BlockchainBalanceResult> {balanceResult};
        }
    }
}
