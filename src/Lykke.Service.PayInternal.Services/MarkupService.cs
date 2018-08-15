using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayVolatility.Client;
using Lykke.Service.PayVolatility.Models;

namespace Lykke.Service.PayInternal.Services
{
    public class MarkupService : IMarkupService
    {
        private readonly IMarkupRepository _markupRepository;
        private readonly IPayVolatilityClient _payVolatilityClient;

        public MarkupService(IMarkupRepository markupRepository, IPayVolatilityClient payVolatilityClient)
        {
            _markupRepository = markupRepository;
            _payVolatilityClient = payVolatilityClient;
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
            await SetDeltaSpread(markups);
            return markups;
        }

        public async Task<IMarkup> GetDefaultAsync(string assetPairId)
        {
            var markup = await _markupRepository.GetByIdentityAsync(MarkupIdentityType.None, string.Empty, assetPairId);
            await SetDeltaSpread(markup);
            return markup;
        }

        public async Task<IReadOnlyList<IMarkup>> GetForMerchantAsync(string merchantId)
        {
            var markups = await _markupRepository.GetByIdentityAsync(MarkupIdentityType.Merchant, merchantId);
            await SetDeltaSpread(markups);
            return markups;
        }

        public async Task<IMarkup> GetForMerchantAsync(string merchantId, string assetPairId)
        {
            var markup = await _markupRepository.GetByIdentityAsync(MarkupIdentityType.Merchant, merchantId, assetPairId);
            await SetDeltaSpread(markup);
            return markup;
        }

        public async Task<IMarkup> ResolveAsync(string merchantId, string assetPairId)
        {
            IMarkup markup = await GetForMerchantAsync(merchantId, assetPairId);

            return markup ?? await GetDefaultAsync(assetPairId) ??
                   throw new MarkupNotFoundException(merchantId, assetPairId);
        }

        private async Task SetDeltaSpread(IMarkup markup)
        {
            string assetPairId = GetVolatilityAssetPairId(markup);
            if (string.IsNullOrEmpty(assetPairId))
            {
                return;
            }

            VolatilityModel volatilityModel = await _payVolatilityClient.GetDailyVolatilityAsync(assetPairId);
            if (volatilityModel != null)
            {
                SetDeltaSpread(markup, volatilityModel);
            }
        }

        private async Task SetDeltaSpread(IEnumerable<IMarkup> markups)
        {
            var volatilityModels = (await _payVolatilityClient.GetDailyVolatilitiesAsync()).ToDictionary(v=>v.AssetPairId,StringComparer.OrdinalIgnoreCase);
            foreach (var markup in markups)
            {
                string assetPairId = GetVolatilityAssetPairId(markup);
                if (string.IsNullOrEmpty(assetPairId))
                {
                    continue;
                }

                if (volatilityModels.TryGetValue(assetPairId, out var volatilityModel))
                {
                    SetDeltaSpread(markup, volatilityModel);
                }
            }
        }

        private void SetDeltaSpread(IMarkup markup, VolatilityModel volatilityModel)
        {
            markup.DeltaSpread = volatilityModel.HighPriceVolatilityShield;
        }

        private string GetVolatilityAssetPairId(IMarkup markup)
        {
            if (!string.IsNullOrEmpty(markup?.PriceAssetPairId))
            {
                return markup.PriceAssetPairId;
            }

            return markup?.AssetPairId;
        }
    }
}
