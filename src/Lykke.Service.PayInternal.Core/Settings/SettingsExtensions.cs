using System.Collections.Generic;
using System.Linq;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Core.Settings
{
    public static class SettingsExtensions
    {
        public static bool HasAssetPair(this IReadOnlyList<AssetPairSetting> src, string baseAssetId,
            string quotingAssetId)
        {
            return src.Any(x => x.BaseAssetId == baseAssetId && x.QuotingAssetId == quotingAssetId);
        }
    }
}
