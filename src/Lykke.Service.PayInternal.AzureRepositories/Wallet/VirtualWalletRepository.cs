using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.AzureRepositories.Wallet
{
    public class VirtualWalletRepository : IVirtualWalletRepository
    {
        private readonly INoSQLTableStorage<VirtualWalletEntity> _tableStorage;

        public VirtualWalletRepository(
            INoSQLTableStorage<VirtualWalletEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IVirtualWallet> CreateAsync(IVirtualWallet wallet)
        {
            VirtualWalletEntity entity = VirtualWalletEntity.ByMerchantId.Create(wallet);

            await _tableStorage.InsertAsync(entity);

            return Mapper.Map<VirtualWalletEntity, VirtualWallet>(entity);
        }
    }
}
