using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.Refund;

namespace Lykke.Service.PayInternal.AzureRepositories.Refund
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class RefundEntity : AzureTableEntity, IRefund
    {
        public string RefundId { get; set; }
        public string MerchantId { get; set; }
        public string PaymentRequestId { get; set; }
        public string SettlementId { get; set; }
        public decimal Amount { get; set; }
        
        public void Map(IRefund refund)
        {
            PartitionKey = refund.MerchantId;
            RowKey = refund.RefundId;

            PaymentRequestId = refund.PaymentRequestId;
            RefundId = refund.RefundId;
            MerchantId = refund.MerchantId;
            SettlementId = refund.SettlementId;
            Amount = refund.Amount;
        }
    }
}
