using System;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.PayInternal.AzureRepositories.Order
{
    public class OrderEntity : TableEntity, IOrder
    {
        public static class ByMerchant
        {
            public static string GeneratePartitionKey(string merchantId)
            {
                return merchantId;
            }

            public static string GenerateRowKey()
            {
                return Guid.NewGuid().ToString();
            }

            public static OrderEntity Create(IOrder src)
            {
                return new OrderEntity
                {
                    PartitionKey = GeneratePartitionKey(src.MerchantId),
                    RowKey = GenerateRowKey(),
                    MerchantId = src.MerchantId,
                    AssetPairId = src.AssetPairId,
                    DueDate = src.DueDate,
                    ExchangeAmount = src.ExchangeAmount,
                    ExchangeAssetId = src.ExchangeAssetId,
                    InvoiceAmount = src.InvoiceAmount,
                    InvoiceAssetId = src.InvoiceAssetId,
                    InvoiceId = src.InvoiceId,
                    MarkupPercent = src.MarkupPercent,
                    MarkupPips = src.MarkupPips,
                    ErrorUrl = src.ErrorUrl,
                    ProgressUrl = src.ProgressUrl,
                    SuccessUrl = src.SuccessUrl,
                    WalletAddress = src.WalletAddress
                };
            }
        }
        
        public string Id => RowKey;
        public string MerchantId { get; set; }
        public string InvoiceId { get; set; }
        public string AssetPairId { get; set; }
        public string InvoiceAssetId { get; set; }
        public double InvoiceAmount { get; set; }
        public string ExchangeAssetId { get; set; }
        public double ExchangeAmount { get; set; }
        public DateTime DueDate { get; set; }
        public double MarkupPercent { get; set; }
        public int MarkupPips { get; set; }
        public string SuccessUrl { get; set; }
        public string ErrorUrl { get; set; }
        public string ProgressUrl { get; set; }
        public string WalletAddress { get; set; }
    }
}
