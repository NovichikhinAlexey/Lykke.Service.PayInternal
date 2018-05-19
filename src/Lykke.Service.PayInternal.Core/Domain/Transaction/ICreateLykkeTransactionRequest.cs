namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface ICreateLykkeTransactionRequest
    {
        string OperationId { get; set; }

        string[] SourceWalletAddresses { get; set; }

        decimal Amount { get; set; }

        string AssetId { get; set; }

        int Confirmations { get; set; }

        TransactionIdentityType IdentityType { get; set; }

        string Identity { get; set; }
    }
}
