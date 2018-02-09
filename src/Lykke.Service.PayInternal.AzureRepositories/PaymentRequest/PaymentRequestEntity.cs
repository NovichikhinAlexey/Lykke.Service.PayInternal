using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;

namespace Lykke.Service.PayInternal.AzureRepositories.PaymentRequest
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class PaymentRequestEntity : AzureTableEntity, IPaymentRequest
    {
        private decimal _amount;
        private DateTime _dueDate;
        private double _markupPercent;
        private int _markupPips;
        private decimal _paidAmount;
        private double _markupFixedFee;
        
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
        
        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                MarkValueTypePropertyAsDirty(nameof(Amount));
            }
        }
        
        public string SettlementAssetId { get; set; }
        
        public string PaymentAssetId { get; set; }
        
        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                MarkValueTypePropertyAsDirty(nameof(DueDate));
            }
        }
       
        public double MarkupPercent
        {
            get => _markupPercent;
            set
            {
                _markupPercent = value;
                MarkValueTypePropertyAsDirty(nameof(MarkupPercent));
            }
        }
        
        public int MarkupPips
        {
            get => _markupPips;
            set
            {
                _markupPips = value;
                MarkValueTypePropertyAsDirty(nameof(MarkupPips));
            }
        }

        public double MarkupFixedFee
        {
            get => _markupFixedFee;
            set
            {
                _markupFixedFee = value;
                MarkValueTypePropertyAsDirty(nameof(MarkupFixedFee));
            }
        }
        
        public string WalletAddress { get; set; }
        
        public PaymentRequestStatus Status { get; set; }
        
        public decimal PaidAmount
        {
            get => _paidAmount;
            set
            {
                _paidAmount = value;
                MarkValueTypePropertyAsDirty(nameof(PaidAmount));
            }
        }
        
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
            MarkupFixedFee = paymentRequest.MarkupFixedFee;
            WalletAddress = paymentRequest.WalletAddress;
            Status = paymentRequest.Status;
            PaidAmount = paymentRequest.PaidAmount;
            PaidDate = paymentRequest.PaidDate;
            Error = paymentRequest.Error;
        }
    }
}
