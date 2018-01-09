using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IMerchantWalletsService
    {
        Task<string> CreateAddress(string merchantId);
    }
}
