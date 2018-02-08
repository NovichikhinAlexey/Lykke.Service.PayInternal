﻿using System;
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
                    DueDate = src.DueDate,
                    Amount = src.Amount,
                    PublicKey = src.PublicKey
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
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string PublicKey { get; set; }
    }
}