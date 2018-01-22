using System;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.PayInternal.AzureRepositories.Order
{
    public class OrderEntity : TableEntity, IOrder
    {
        public static class ByWallet
        {
            public static string GeneratePartitionKey(string address)
            {
                return address;
            }

            public static string GenerateRowKey()
            {
                return Guid.NewGuid().ToString();
            }

            public static OrderEntity Create(IOrder src)
            {
                return new OrderEntity
                {
                    PartitionKey = GeneratePartitionKey(src.WalletAddress),
                    RowKey = GenerateRowKey(),
                    MerchantId = src.MerchantId,
                    AssetPairId = src.AssetPairId,
                    DueDate = src.DueDate,
                    ExchangeAmount = src.ExchangeAmount,
                    ExchangeAssetId = src.ExchangeAssetId,
                    InvoiceAmount = src.InvoiceAmount,
                    InvoiceAssetId = src.InvoiceAssetId,
                    MarkupPercent = src.MarkupPercent,
                    MarkupPips = src.MarkupPips,
                    WalletAddress = src.WalletAddress
                };
            }
        }
        
        public string Id => RowKey;
        public string MerchantId { get; set; }
        public string AssetPairId { get; set; }
        public string InvoiceAssetId { get; set; }
        public double InvoiceAmount { get; set; }
        public string ExchangeAssetId { get; set; }
        public double ExchangeAmount { get; set; }
        public DateTime DueDate { get; set; }
        public double MarkupPercent { get; set; }
        public int MarkupPips { get; set; }
        public string WalletAddress { get; set; }
    }
}
