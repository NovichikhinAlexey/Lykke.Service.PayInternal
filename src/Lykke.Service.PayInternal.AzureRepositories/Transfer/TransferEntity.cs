using Lykke.AzureStorage.Tables;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using System;

namespace Lykke.Service.PayInternal.AzureRepositories.Transfer
{
    public class TransferEntity : AzureTableEntity, IMultipartTransfer
    {
        public string MerchantId { get; set; }

        public DateTime CreationDate { get; set; }

        public string AssetId { get; set; }

        public int FeeRate { get; set; }

        public decimal FixedFee { get; set; }

        public string TransferId { get; set; }

        public string PaymentRequestId { get; set; }

        public void Map(IMultipartTransfer transfer)
        {
            PaymentRequestId = transfer.PaymentRequestId;
            TransferId = transfer.TransferId;

            PartitionKey = transfer.PaymentRequestId;
            RowKey = transfer.TransferId;
            MerchantId = transfer.MerchantId;
            CreationDate = transfer.CreationDate;
            AssetId = transfer.AssetId;
            FeeRate = transfer.FeeRate;
            FixedFee = transfer.FixedFee;
        }
    }
}
