using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.PayInternal.AzureRepositories;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayMerchant.Client;
using Lykke.Service.PayMerchant.Client.Models;
using Lykke.Service.PayVolatility.Client;
using Lykke.Service.PayVolatility.Models;

namespace Lykke.Service.PayInternal.Services
{
    public class MarkupService : IMarkupService
    {
        private class MerchantVolatilitySettings
        {
            public bool IsDeltaSpreadFixed { get; set; }
            public List<string> ZeroCoverageAssetPairs { get; set; }
        }

        private readonly IMarkupRepository _markupRepository;
        private readonly IPayVolatilityClient _payVolatilityClient;
        private readonly IPayMerchantClient _payMerchantClient;
        private readonly string[] _volatilityAssetPairs;
        private readonly ILog _log;

        public MarkupService(
            [NotNull] IMarkupRepository markupRepository, 
            [NotNull] IPayVolatilityClient payVolatilityClient, 
            [NotNull] IPayMerchantClient payMerchantClient,
            [NotNull] string[] volatilityAssetPairs,
            [NotNull] ILogFactory logFactory)
        {
            _markupRepository = markupRepository;
            _payVolatilityClient = payVolatilityClient;
            _payMerchantClient = payMerchantClient;
            _volatilityAssetPairs = volatilityAssetPairs;
            _log = logFactory.CreateLog(this);
        }

        public Task<IMarkup> SetDefaultAsync(string assetPairId, string priceAssetPairId, PriceMethod priceMethod, IMarkupValue markupValue)
        {
            return _markupRepository.SetAsync(new Markup
            {
                AssetPairId = assetPairId,
                CreatedOn = DateTime.UtcNow,
                DeltaSpread = markupValue.DeltaSpread,
                Percent = markupValue.Percent,
                Pips = markupValue.Pips,
                FixedFee = markupValue.FixedFee,
                PriceAssetPairId = priceAssetPairId,
                PriceMethod = priceMethod,
                IdentityType = MarkupIdentityType.None,
                Identity = string.Empty
            });
        }

        public Task<IMarkup> SetForMerchantAsync(string assetPairId, string merchantId, string priceAssetPairId, PriceMethod priceMethod, IMarkupValue markupValue)
        {
            return _markupRepository.SetAsync(new Markup
            {
                AssetPairId = assetPairId,
                CreatedOn = DateTime.UtcNow,
                DeltaSpread = markupValue.DeltaSpread,
                Percent = markupValue.Percent,
                Pips = markupValue.Pips,
                FixedFee = markupValue.FixedFee,
                PriceAssetPairId = priceAssetPairId,
                PriceMethod = priceMethod,
                IdentityType = MarkupIdentityType.Merchant,
                Identity = merchantId
            });
        }

        public async Task<IReadOnlyList<IMarkup>> GetDefaultsAsync()
        {
            var markups = await _markupRepository.GetByIdentityAsync(MarkupIdentityType.None, string.Empty);
            await SetDeltaSpreadAsync(markups);
            return markups;
        }

        public async Task<IMarkup> GetDefaultAsync(string assetPairId)
        {
            var markup = await _markupRepository.GetByIdentityAsync(MarkupIdentityType.None, string.Empty, assetPairId);
            await SetDeltaSpreadAsync(markup, false);
            return markup;
        }

        public async Task<IReadOnlyList<IMarkup>> GetForMerchantAsync(string merchantId)
        {
            var markups = await _markupRepository.GetByIdentityAsync(MarkupIdentityType.Merchant, merchantId);

            MerchantVolatilitySettings volatilitySettings = await GetVolatilityMerchantSettingsAsync(merchantId);

            foreach (var markup in markups)
            {
                if (volatilitySettings.ZeroCoverageAssetPairs.Contains(markup.AssetPairId))
                {
                    markup.DeltaSpread = 0;
                }
                else
                {
                    await SetDeltaSpreadAsync(markup, volatilitySettings.IsDeltaSpreadFixed);
                }
            }

            return markups;
        }

        public async Task<IMarkup> GetForMerchantAsync(string merchantId, string assetPairId)
        {
            var markup = await _markupRepository.GetByIdentityAsync(MarkupIdentityType.Merchant, merchantId, assetPairId);

            MerchantVolatilitySettings volatilitySettings = await GetVolatilityMerchantSettingsAsync(merchantId);

            if (markup != null)
            {
                if (volatilitySettings.ZeroCoverageAssetPairs.Contains(assetPairId))
                {
                    markup.DeltaSpread = 0;
                }
                else
                {
                    await SetDeltaSpreadAsync(markup, volatilitySettings.IsDeltaSpreadFixed);
                }
            }

            return markup;
        }

        public async Task<IMarkup> ResolveAsync(string merchantId, string assetPairId)
        {
            IMarkup markup = await GetForMerchantAsync(merchantId, assetPairId);

            return markup ?? await GetDefaultAsync(assetPairId) ??
                   throw new MarkupNotFoundException(merchantId, assetPairId);
        }

        private async Task SetDeltaSpreadAsync(IMarkup markup, bool isDeltaSpreadFixed)
        {
            string assetPairId = GetVolatilityAssetPairId(markup);
            if (string.IsNullOrEmpty(assetPairId) 
                || !_volatilityAssetPairs.Contains(assetPairId, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            VolatilityModel volatilityModel;
            try
            {
                volatilityModel = await _payVolatilityClient.Volatility.GetDailyVolatilityAsync(assetPairId);
            }
            catch (Refit.ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _log.Critical(ex, $"There are no volatility of {assetPairId} for last days.");
                return;
            }

            if (volatilityModel != null)
            {
                SetDeltaSpread(markup, volatilityModel, isDeltaSpreadFixed);
            }
        }

        private Task SetDeltaSpreadAsync(IEnumerable<IMarkup> markups)
        {
            var tasks = new List<Task>();
            foreach (var markup in markups)
            {
                tasks.Add(SetDeltaSpreadAsync(markup, false));
            }

            return Task.WhenAll(tasks);
        }

        private void SetDeltaSpread(IMarkup markup, VolatilityModel volatilityModel, bool isDeltaSpreadFixed)
        {
            markup.DeltaSpread = isDeltaSpreadFixed
                ? Math.Max(markup.DeltaSpread, volatilityModel.HighPriceVolatilityShield)
                : volatilityModel.HighPriceVolatilityShield;
        }

        private string GetVolatilityAssetPairId(IMarkup markup)
        {
            if (!string.IsNullOrEmpty(markup?.PriceAssetPairId))
            {
                return markup.PriceAssetPairId;
            }

            return markup?.AssetPairId;
        }

        private async Task<MerchantVolatilitySettings> GetVolatilityMerchantSettingsAsync(string merchantId)
        {
            try
            {
                VolatilitySettingsResponse response =
                    await _payMerchantClient.Settings.GetVolatilitySettingsAsync(merchantId);

                return new MerchantVolatilitySettings
                {
                    IsDeltaSpreadFixed = response.IsDeltaSpreadFixed,
                    ZeroCoverageAssetPairs = response.ZeroCoverageAssetPairs.Split(Constants.Separator).ToList()
                };
            }
            catch (ClientApiException e) when (e.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return new MerchantVolatilitySettings
                {
                    IsDeltaSpreadFixed = default(bool),
                    ZeroCoverageAssetPairs = new List<string>()
                };
            }
        }
    }
}
