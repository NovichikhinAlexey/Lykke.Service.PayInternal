using System;
using Lykke.AzureStorage.Tables;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;

namespace Lykke.Service.PayInternal.AzureRepositories.PaymentRequest
{
    public class PaymentRequestEntity : AzureTableEntity, IPaymentRequest
    {
        public PaymentRequestEntity()
        {
        }

        public PaymentRequestEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string Id => RowKey;
        public string MerchantId { get; set; }
        public string OrderId { get; set; }
        public double Amount { get; set; }
        public string SettlementAssetId { get; set; }
        public string PaymentAssetId { get; set; }
        public DateTime DueDate { get; set; }
        public double MarkupPercent { get; set; }
        public int MarkupPips { get; set; }
        public string WalletAddress { get; set; }
        public PaymentRequestStatus Status { get; set; }
        public double PaidAmount { get; set; }
        public DateTime? PaidDate { get; set; }
        public string Error { get; set; }
        
        internal void Map(IPaymentRequest paymentRequest)
        {
            MerchantId = paymentRequest.MerchantId;
            OrderId = paymentRequest.OrderId;
            Amount = paymentRequest.Amount;
            SettlementAssetId = paymentRequest.SettlementAssetId;
            PaymentAssetId = paymentRequest.PaymentAssetId;
            DueDate = paymentRequest.DueDate;
            MarkupPercent = paymentRequest.MarkupPercent;
            MarkupPips = paymentRequest.MarkupPips;
            WalletAddress = paymentRequest.WalletAddress;
            Status = paymentRequest.Status;
            PaidAmount = paymentRequest.PaidAmount;
            PaidDate = paymentRequest.PaidDate;
            Error = paymentRequest.Error;
        }
    }
}
