using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.MarketProfile.Client;
using Lykke.Service.MarketProfile.Client.Models;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class AssetRatesService : IAssetRatesService
    {
        private readonly IAssetPairRateRepository _assetPairRateRepository;
        private readonly ILykkeMarketProfile _marketProfileServiceClient;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly IAssetPairSettingsService _assetPairSettingsService;

        public AssetRatesService(
            [NotNull] IAssetPairRateRepository assetPairRateRepository, 
            [NotNull] ILykkeMarketProfile marketProfileServiceClient, 
            [NotNull] IAssetsLocalCache assetsLocalCache, 
            [NotNull] IAssetPairSettingsService assetPairSettingsService)
        {
            _assetPairRateRepository = assetPairRateRepository ?? throw new ArgumentNullException(nameof(assetPairRateRepository));
            _marketProfileServiceClient = marketProfileServiceClient ?? throw new ArgumentNullException(nameof(marketProfileServiceClient));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _assetPairSettingsService = assetPairSettingsService ?? throw new ArgumentNullException(nameof(assetPairSettingsService));
        }

        public async Task<IAssetPairRate> AddAsync(AddAssetPairRateCommand cmd)
        {
            AssetPairSetting settings = await _assetPairSettingsService.GetAsync(cmd.BaseAssetId, cmd.QuotingAssetId);

            if (settings == null)
                throw new AssetPairRateStorageNotSupportedException(cmd.BaseAssetId, cmd.QuotingAssetId);

            await _assetPairRateRepository.AddAsync(
                Mapper.Map<AssetPairRate>(cmd.Invert().ApplyAccuracy(settings.Accuracy)));

            return await _assetPairRateRepository.AddAsync(
                Mapper.Map<AssetPairRate>(cmd.ApplyAccuracy(settings.Accuracy)));
        }

        public async Task<IAssetPairRate> GetCurrentRateAsync(string baseAssetId, string quotingAssetId)
        {
            if (await _assetPairSettingsService.GetAsync(baseAssetId, quotingAssetId) != null)
            {
                IReadOnlyList<IAssetPairRate> allRates =
                    await _assetPairRateRepository.GetAsync(baseAssetId, quotingAssetId);

                return allRates
                    .Where(x => x.CreatedOn <= DateTime.UtcNow)
                    .OrderByDescending(x => x.CreatedOn)
                    .FirstOrDefault();
            }

            AssetPair assetPair = await _assetsLocalCache.GetAssetPairAsync(baseAssetId, quotingAssetId);

            if (assetPair == null)
                throw new AssetPairUnknownException(baseAssetId, quotingAssetId);

            AssetPairModel assetPairRate = await InvokeMarketProfileServiceAsync(assetPair.Id);

            return Mapper.Map<AssetPairRate>(assetPairRate, opt =>
            {
                opt.Items["BaseAssetId"] = baseAssetId;
                opt.Items["QuotingAssetId"] = quotingAssetId;
            });
        }

        private async Task<AssetPairModel> InvokeMarketProfileServiceAsync(string assetPairId)
        {
            object response = await _marketProfileServiceClient.ApiMarketProfileByPairCodeGetAsync(assetPairId);

            if (response is ErrorModel error)
            {
                throw new Exception(error.Message);
            }

            if (response is AssetPairModel assetPairRate)
            {
                return assetPairRate;
            }

            throw new UnrecognizedApiResponse("Unknown MarketProfile API response");
        }
    }
}
