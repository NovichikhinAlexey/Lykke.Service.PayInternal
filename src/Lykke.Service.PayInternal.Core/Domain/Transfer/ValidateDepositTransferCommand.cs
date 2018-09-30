namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class ValidateDepositTransferCommand
    {
        public BlockchainType Blockchain { get; set; }

        public string WalletAddress { get; set; }

        public decimal TransferAmount { get; set; }
    }
}
