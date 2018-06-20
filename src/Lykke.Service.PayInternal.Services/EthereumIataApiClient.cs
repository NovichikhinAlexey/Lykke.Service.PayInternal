using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.EthereumCore.Client;
using Lykke.Service.EthereumCore.Client.Models;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class EthereumIataApiClient : IBlockchainApiClient
    {
        private readonly IEthereumCoreAPI _ethereumServiceClient;
        private readonly EthereumBlockchainSettings _ethereumSettings;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly IAssetsService _assetsService;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly ILog _log;

        public EthereumIataApiClient(
            [NotNull] IEthereumCoreAPI ethereumServiceClient, 
            [NotNull] EthereumBlockchainSettings ethereumSettings, 
            [NotNull] IAssetsLocalCache assetsLocalCache, 
            [NotNull] IAssetsService assetsService, 
            [NotNull] ILykkeAssetsResolver lykkeAssetsResolver, 
            [NotNull] ILog log)
        {
            _ethereumServiceClient = ethereumServiceClient ?? throw new ArgumentNullException(nameof(ethereumServiceClient));
            _ethereumSettings = ethereumSettings ?? throw new ArgumentNullException(nameof(ethereumSettings));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _assetsService = assetsService ?? throw new ArgumentNullException(nameof(assetsService));
            _lykkeAssetsResolver = lykkeAssetsResolver ?? throw new ArgumentNullException(nameof(lykkeAssetsResolver));
            _log = log.CreateComponentScope(nameof(EthereumIataApiClient)) ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<BlockchainTransferResult> TransferAsync(BlockchainTransferCommand transfer)
        {
            BlockchainTransferResult result = new BlockchainTransferResult {Blockchain = BlockchainType.EthereumIata};

            string lykkeAssetId = await _lykkeAssetsResolver.GetLykkeId(transfer.AssetId);

            Asset asset = await _assetsLocalCache.GetAssetByIdAsync(lykkeAssetId);

            if (asset.Type != AssetType.Erc20Token)
                throw new AssetNotSupportedException(asset.Name);

            ListOfErc20Token tokenSpecification =
                await _assetsService.Erc20TokenGetBySpecificationAsync(new Erc20TokenSpecification {Ids = new[] {asset.Id}});

            string tokenAddress = tokenSpecification?.Items.SingleOrDefault(x => x.AssetId == asset.Id)?.Address;

            foreach (TransferAmount transferAmount in transfer.Amounts)
            {
                var transferRequest = Mapper.Map<AirlinesTransferFromDepositRequest>(transferAmount,
                    opts => opts.Items["TokenAddress"] = tokenAddress);

                object response = await _ethereumServiceClient.ApiAirlinesErc20depositsTransferPostAsync(
                    _ethereumSettings.ApiKey, transferRequest);

                var errorMessage = string.Empty;

                var operationId = string.Empty;

                if (response is ApiException ex)
                {
                    _log.WriteWarning(nameof(TransferAsync), transferAmount, ex.Error?.ToJson());

                    errorMessage = ex.Error?.Message;
                }
                else if (response is OperationIdResponse op)
                {
                    operationId = op.OperationId;
                }
                else
                {
                    throw new UnrecognizedApiResponse(response?.GetType().FullName ?? "Response object is null");
                }

                result.Transactions.Add(new BlockchainTransactionResult
                {
                    Amount = transferAmount.Amount,
                    AssetId = asset.DisplayId,
                    Hash = string.Empty,
                    IdentityType = TransactionIdentityType.Specific,
                    Identity = operationId,
                    Sources = new List<string> { transferAmount.Source },
                    Destinations = new List<string> { transferAmount.Destination },
                    Error = errorMessage
                });
            }

            return result;
        }

        public async Task<string> CreateAddressAsync()
        {
            object response =
                await _ethereumServiceClient.ApiAirlinesErc20depositsPostAsync(_ethereumSettings.ApiKey, StringUtils.GenerateId());

            if (response is ApiException ex)
            {
                _log.WriteWarning(nameof(CreateAddressAsync), "New erc20 address generation",
                    ex.Error?.Message);

                throw new WalletAddressAllocationException(BlockchainType.EthereumIata);
            }

            if (response is RegisterResponse result)
            {
                return result.Contract;
            }

            throw new UnrecognizedApiResponse(response?.GetType().FullName);
        }

        public async Task<bool> ValidateAddressAsync(string address)
        {
            object response = await _ethereumServiceClient.ApiValidationGetAsync(address);

            if (response is ApiException ex)
            {
                _log.WriteWarning(nameof(ValidateAddressAsync), "EthereumIata address validation",
                    ex.Error?.Message);

                throw new WalletAddressValidationException(BlockchainType.EthereumIata, address);
            }

            if (response is IsAddressValidResponse result)
            {
                return result.IsValid;
            }

            throw new UnrecognizedApiResponse(response?.GetType().FullName);
        }

        public async Task<IReadOnlyList<BlockchainBalanceResult>> GetBalancesAsync(string address)
        {
            var balances = new List<BlockchainBalanceResult>();

            object response = await _ethereumServiceClient.ApiErc20BalancePostAsync(new GetErcBalance(address));

            if (response is ApiException ex)
            {
                _log.WriteWarning(nameof(GetBalancesAsync), "EthereumIata balances",
                    ex.Error?.Message);

                throw new WalletAddressBalanceException(BlockchainType.EthereumIata, address);
            }

            if (response is AddressTokenBalanceContainerResponse result)
            {
                foreach (AddressTokenBalanceResponse balanceResponse in result.Balances)
                {
                    Erc20Token token =
                        await _assetsService.Erc20TokenGetByAddressAsync(balanceResponse.Erc20TokenAddress);

                    if (string.IsNullOrEmpty(token?.AssetId))
                        continue;

                    Asset asset = await _assetsLocalCache.GetAssetByIdAsync(token.AssetId);

                    if (asset == null) 
                        continue;

                    balances.Add(new BlockchainBalanceResult
                    {
                        AssetId = asset.DisplayId,
                        Balance = balanceResponse.Balance.ToAmount(asset.MultiplierPower, asset.Accuracy)
                    });
                }

                return balances;

            }

            throw new UnrecognizedApiResponse(response?.GetType().FullName);
        }
    }
}
