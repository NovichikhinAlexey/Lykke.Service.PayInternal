using System;
using JetBrains.Annotations;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IPaymentRequestTransaction
    {
        string Id { get; }

        //todo: rename to hash
        string TransactionId { get; set; }

        string TransferId { get; set; }

        string PaymentRequestId { get; set; }

        decimal Amount { get; set; }

        string AssetId { get; set; }

        [CanBeNull] string BlockId { get; set; }

        BlockchainType Blockchain { get; set; }

        int Confirmations { get; set; }

        string WalletAddress { get; set; }

        string[] SourceWalletAddresses { get; set; }

        DateTime? FirstSeen { get; set; }

        DateTime CreatedOn { get; set; }

        TransactionType TransactionType { get; set; }

        DateTime? DueDate { get; set; }

        TransactionIdentityType IdentityType { get; set; }

        string ContextData { get; set; }

        string Identity { get; set; }

        bool IsPayment();

        bool IsSettlement();

        bool IsRefund();

        bool Confirmed(int confirmationLimit);

        bool Expired();
    }
}
