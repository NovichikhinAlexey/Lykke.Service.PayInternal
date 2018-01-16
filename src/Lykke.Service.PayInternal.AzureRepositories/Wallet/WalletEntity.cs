using System;
using System.Globalization;
using Common;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.PayInternal.AzureRepositories.Wallet
{
    public class WalletEntity : TableEntity, IWallet
    {
        public static class ByMerchant
        {
            public static string GeneratePartitionKey(string merchantId)
            {
                return merchantId;
            }

            public static string GenerateRowKey(string address)
            {
                return address;
            }

            public static WalletEntity Create(IWallet src)
            {
                return new WalletEntity
                {
                    PartitionKey = GeneratePartitionKey(src.MerchantId),
                    RowKey = GenerateRowKey(src.Address),
                    MerchantId = src.MerchantId,
                    Address = src.Address,
                    Data = src.Data,
                    DueDate = src.DueDate,
                    Amount = src.Amount
                };
            }
        }

        public static class ByDueDate
        {
            public static string GeneratePartitionKey(DateTime dueDate)
            {
                var dueDateIso = dueDate.ToString("O");

                return $"DD_{dueDateIso}";
            }

            public static string GenerateRowKey(string address)
            {
                return address;
            }

            public static WalletEntity Create(IWallet src)
            {
                return new WalletEntity
                {
                    PartitionKey = GeneratePartitionKey(src.DueDate),
                    RowKey = GenerateRowKey(src.Address),
                    MerchantId = src.MerchantId,
                    Amount = src.Amount,
                    DueDate = src.DueDate,
                    Address = src.Address
                };
            }
        }

        public string MerchantId { get; set; }
        public string Address { get; set; }
        public string Data { get; set; }
        public DateTime DueDate { get; set; }
        public double Amount { get; set; }
    }
}
