using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ILykkeAssetsResolver
    {
        Task<string> GetLykkeId(string assetId);
    }
}
