using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class MarkupService : IMarkupService
    {
        private readonly IMarkupRepository _markupRepository;

        public MarkupService(IMarkupRepository markupRepository)
        {
            _markupRepository = markupRepository;
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

        public Task<IReadOnlyList<IMarkup>> GetDefaultsAsync()
        {
            return _markupRepository.GetByIdentityAsync(MarkupIdentityType.None, string.Empty);
        }

        public Task<IMarkup> GetDefaultAsync(string assetPairId)
        {
            return _markupRepository.GetByIdentityAsync(MarkupIdentityType.None, string.Empty, assetPairId);
        }

        public Task<IReadOnlyList<IMarkup>> GetForMerchantAsync(string merchantId)
        {
            return _markupRepository.GetByIdentityAsync(MarkupIdentityType.Merchant, merchantId);
        }

        public Task<IMarkup> GetForMerchantAsync(string merchantId, string assetPairId)
        {
            return _markupRepository.GetByIdentityAsync(MarkupIdentityType.Merchant, merchantId, assetPairId);
        }

        public async Task<IMarkup> ResolveAsync(string merchantId, string assetPairId)
        {
            IMarkup markup = await GetForMerchantAsync(merchantId, assetPairId);

            return markup ?? await GetDefaultAsync(assetPairId) ??
                   throw new MarkupNotFoundException(merchantId, assetPairId);
        }
    }
}
