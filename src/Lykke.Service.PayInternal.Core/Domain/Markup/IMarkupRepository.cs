using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Markup
{
    public interface IMarkupRepository
    {
        Task<IMarkup> SetAsync(IMarkup markup);

        Task<IReadOnlyList<IMarkup>> GetByIdentityAsync(MarkupIdentityType identityType, string identity);

        Task<IMarkup> GetByIdentityAsync(MarkupIdentityType identityType, string identity, string assetPairId);

        Task<IReadOnlyList<IMarkup>> GetByAssetAsync(string assetPairId);
    }
}
