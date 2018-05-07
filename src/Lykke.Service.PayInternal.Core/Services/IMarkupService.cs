using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Markup;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IMarkupService
    {
        Task<IMarkup> SetDefaultAsync(string assetPairId, IMarkupValue markupValue);

        Task<IMarkup> SetForMerchantAsync(string assetPairId, string merchantId, IMarkupValue markupValue);

        Task<IReadOnlyList<IMarkup>> GetDefaultsAsync();

        Task<IMarkup> GetDefaultAsync(string assetPairId);

        Task<IReadOnlyList<IMarkup>> GetForMerchantAsync(string merchantId);

        Task<IMarkup> GetForMerchantAsync(string merchantId, string assetPairId);

        Task<IMarkup> ResolveAsync(string merchantId, string assetPairId);
    }
}
