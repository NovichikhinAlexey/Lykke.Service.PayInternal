namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class ValidateDepositRawTransferCommand : IBlockchainTypeHolder
    {
        public BlockchainType Blockchain { get; set; }

        public string BlockchainWalletAddress { get; set; }

        public decimal TransferAmount { get; set; }
    }
}
