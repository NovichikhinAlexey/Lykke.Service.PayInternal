using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Services.Domain;

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

        public static IMerchantMarkup GetMarkup(this IMerchant src)
        {
            return new MerchantMarkup
            {
                LpPercent = src.LpMarkupPercent,
                DeltaSpread = src.DeltaSpread,
                LpPips = src.LpMarkupPips,
                LpFixedFee = src.MarkupFixedFee
            };
        }
    }
}
