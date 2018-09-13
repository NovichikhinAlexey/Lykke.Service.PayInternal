using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
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
using Microsoft.Rest;
using Polly;
using ExceptionType = Lykke.Service.EthereumCore.Client.Models.ExceptionType;

namespace Lykke.Service.PayInternal.Services
{
    public class EthereumIataApiClient : IBlockchainApiClient
    {
        private readonly IEthereumCoreAPI _ethereumServiceClient;
        private readonly EthereumBlockchainSettings _ethereumSettings;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly IAssetsService _assetsService;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly RetryPolicySettings _retryPolicySettings;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Policy<object> _retryPolicy;
        private readonly ILog _log;

        public EthereumIataApiClient(
            [NotNull] IEthereumCoreAPI ethereumServiceClient, 
            [NotNull] EthereumBlockchainSettings ethereumSettings, 
            [NotNull] IAssetsLocalCache assetsLocalCache, 
            [NotNull] IAssetsService assetsService,
            [NotNull] ILykkeAssetsResolver lykkeAssetsResolver, 
            [NotNull] ILogFactory logFactory, 
            [NotNull] RetryPolicySettings retryPolicySettings)
        {
            _ethereumServiceClient = ethereumServiceClient ?? throw new ArgumentNullException(nameof(ethereumServiceClient));
            _ethereumSettings = ethereumSettings ?? throw new ArgumentNullException(nameof(ethereumSettings));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _assetsService = assetsService ?? throw new ArgumentNullException(nameof(assetsService));
            _lykkeAssetsResolver = lykkeAssetsResolver ?? throw new ArgumentNullException(nameof(lykkeAssetsResolver));
            _retryPolicySettings = retryPolicySettings ?? throw new ArgumentNullException(nameof(retryPolicySettings));
            _log = logFactory.CreateLog(this);
            _retryPolicy = Policy
                .HandleResult<object>(r =>
                {
                    if (r is ApiException apiException)
                    {
                        return apiException.Error?.Code == ExceptionType.None;
                    }

                    return false;
                })
                .WaitAndRetryAsync(
                    _retryPolicySettings.DefaultAttempts,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, timespan) => _log.Error(message: "Connecting ethereum core with retry", context: ex));
        }

        public async Task<BlockchainTransferResult> TransferAsync(BlockchainTransferCommand transfer)
        {
            BlockchainTransferResult result = new BlockchainTransferResult {Blockchain = BlockchainType.EthereumIata};

            string lykkeAssetId = await _lykkeAssetsResolver.GetLykkeId(transfer.AssetId);

            Asset asset = await _assetsLocalCache.GetAssetByIdAsync(lykkeAssetId);

            if (asset.Type != AssetType.Erc20Token)
                throw new AssetNotSupportedException(asset.Name);

            ListOfErc20Token tokenSpecification =
                await _assetsService.Erc20TokenGetBySpecificationAsync(new Erc20TokenSpecification
                    {Ids = new[] {asset.Id}});

            string tokenAddress = tokenSpecification?.Items.SingleOrDefault(x => x.AssetId == asset.Id)?.Address;

            foreach (TransferAmount transferAmount in transfer.Amounts)
            {
                var transferRequest = Mapper.Map<AirlinesTransferFromDepositRequest>(transferAmount,
                    opts =>
                    {
                        opts.Items["TokenAddress"] = tokenAddress;
                        opts.Items["AssetMultiplier"] = asset.MultiplierPower;
                        opts.Items["AssetAccuracy"] = asset.Accuracy;
                    });

                object response = await _retryPolicy.ExecuteAsync(() => InvokeTransfer(transferRequest));

                var ex = response as ApiException;

                var operation = response as OperationIdResponse;

                if (ex != null)
                    _log.Warning(ex.Error?.Message);

                if (ex == null && operation == null)
                    throw new UnrecognizedApiResponse(response?.GetType().FullName ?? "Response object is null");

                result.Transactions.Add(new BlockchainTransactionResult
                {
                    Amount = transferAmount.Amount ?? 0,
                    AssetId = asset.DisplayId,
                    Hash = string.Empty,
                    IdentityType = TransactionIdentityType.Specific,
                    Identity = operation?.OperationId ?? string.Empty,
                    Sources = new List<string> {transferAmount.Source},
                    Destinations = new List<string> {transferAmount.Destination},
                    Error = ex?.Error?.Message ?? string.Empty,
                    ErrorType = ex.GetDomainError()
                });
            }

            return result;
        }

        public async Task<string> CreateAddressAsync()
        {
            object response = await _retryPolicy.ExecuteAsync(() =>
                _ethereumServiceClient.ApiAirlinesErc20depositsPostAsync(
                    _ethereumSettings.ApiKey,
                    StringUtils.GenerateId()));

            if (response is ApiException ex)
            {
                _log.Warning("New erc20 address generation", context: ex.ToJson());

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
            object response =
                await _retryPolicy.ExecuteAsync(() => _ethereumServiceClient.ApiValidationGetAsync(address));

            if (response is ApiException ex)
            {
                _log.Warning("EthereumIata address validation", context: ex.ToJson());

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

            object response = await _retryPolicy.ExecuteAsync(() =>
                _ethereumServiceClient.ApiErc20BalancePostAsync(new GetErcBalance(address)));

            if (response is ApiException ex)
            {
                _log.Error(message: ex.Error?.Message, context: ex.ToJson());

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
                        AssetId = asset.Id,
                        Balance = balanceResponse.Balance.FromContract(asset.MultiplierPower, asset.Accuracy)
                    });
                }

                return balances;

            }

            throw new UnrecognizedApiResponse(response?.GetType().FullName);
        }

        private async Task<object> InvokeTransfer(AirlinesTransferFromDepositRequest request)
        {
            object response;

            try
            {
                response = await _ethereumServiceClient.ApiAirlinesErc20depositsTransferPostAsync(
                    _ethereumSettings.ApiKey, request);
            }
            catch (ValidationException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.Details,
                    e.Rule,
                    e.Target
                });

                response = new ApiException(new ApiError(ExceptionType.WrongParams, e.Message));
            }

            return response;
        }
    }
}
