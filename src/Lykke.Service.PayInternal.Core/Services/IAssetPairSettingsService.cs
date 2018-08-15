using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IAssetPairSettingsService
    {
        Task<AssetPairSetting> GetAsync(string baseAssetId, string quotingAssetId);
    }
}
