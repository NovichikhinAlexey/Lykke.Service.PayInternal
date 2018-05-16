using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Services
{
    public static class ConvertExtensions
    {
        public static BlockchainTransferCommand ToBlockchainTransfer(this TransferCommand src)
        {
            return new BlockchainTransferCommand
            {
                AssetId = src.AssetId,
                Amounts = src.Amounts
            };
        }
    }
}
