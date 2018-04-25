using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IBlockchainAddressValidator
    {
        bool Execute(string address, BlockchainType blockchain);
    }
}
