using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IAssetPairSettingsService
    {
        Task<bool> Contains(string baseAssetId, string quotingAssetId);
    }
}
