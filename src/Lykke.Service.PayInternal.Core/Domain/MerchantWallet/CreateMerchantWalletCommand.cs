namespace Lykke.Service.PayInternal.Core.Domain.MerchantWallet
{
    public class CreateMerchantWalletCommand
    {
        public string MerchantId { get; set; }

        public BlockchainType Network { get; set; }

        public string DisplayName { get; set; }
    }
}
