using System;
using AutoMapper;
using AzureStorage.Tables.Templates.Index;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.AzureRepositories.PaymentRequest
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class PaymentRequestEntity : AzureTableEntity
    {
        private decimal _amount;
        private DateTime _dueDate;
        private double _markupPercent;
        private int _markupPips;
        private decimal _paidAmount;
        private double _markupFixedFee;
        private PaymentRequestProcessingError _processingError;
        private DateTime _createdOn;
        private PaymentRequestStatus _status;
        private DateTime? _paidDate;

        public string Id => RowKey;
        
        public string MerchantId { get; set; }

        public string ExternalOrderId { get; set; }
        
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

        public PaymentRequestStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                MarkValueTypePropertyAsDirty(nameof(Status));
            }
        }
        
        public decimal PaidAmount
        {
            get => _paidAmount;
            set
            {
                _paidAmount = value;
                MarkValueTypePropertyAsDirty(nameof(PaidAmount));
            }
        }

        public DateTime? PaidDate
        {
            get => _paidDate;
            set
            {
                _paidDate = value;
                MarkValueTypePropertyAsDirty(nameof(PaidDate));
            }
        }

        public PaymentRequestProcessingError ProcessingError
        {
            get => _processingError;
            set
            {
                _processingError = value;
                MarkValueTypePropertyAsDirty(nameof(ProcessingError));
            }
        }

        public DateTime CreatedOn
        {
            get => _createdOn;
            set
            {
                _createdOn = value;
                MarkValueTypePropertyAsDirty(nameof(CreatedOn));
            }
        }

        public static class ByMerchant
        {
            public static string GeneratePartitionKey(string merchantId)
            {
                return merchantId;
            }

            public static string GenerateRowKey(string id = null)
            {
                return id ?? Guid.NewGuid().ToString();
            }

            public static PaymentRequestEntity Create(IPaymentRequest paymentRequest)
            {
                var entity = new PaymentRequestEntity
                {
                    PartitionKey = GeneratePartitionKey(paymentRequest.MerchantId),
                    RowKey = GenerateRowKey()
                };

                return Mapper.Map(paymentRequest, entity);
            }
        }

        public static class IndexByWallet
        {
            public static string GeneratePartitionKey(string walletAddress)
            {
                return walletAddress;
            }

            public static string GenerateRowKey()
            {
                return "WalletAddressIndex";
            }

            public static AzureIndex Create(PaymentRequestEntity src)
            {
                return AzureIndex.Create(GeneratePartitionKey(src.WalletAddress), GenerateRowKey(), src);
            }
        }

        public static class IndexByDueDate
        {
            public static string GeneratePartitionKey(DateTime dueDate)
            {
                var dueDateIso = dueDate.ToString("O");

                return $"DD_{dueDateIso}";
            }

            public static string GenerateRowKey(string paymentRequestId)
            {
                return paymentRequestId;
            }

            public static AzureIndex Create(PaymentRequestEntity src)
            {
                return AzureIndex.Create(GeneratePartitionKey(src.DueDate), GenerateRowKey(src.Id), src);
            }
        }
    }
}
