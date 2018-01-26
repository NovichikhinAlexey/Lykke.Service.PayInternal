using System;
using System.Collections.Generic;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Lykke.Service.PayInternal.AzureRepositories.Transfer
{
    public class TransferEntity : TableEntity, ITransferInfo
    {
        public TransferEntity()
        {
            
        }

        public TransferEntity(ITransferInfo transferInfo)
        {
            TransferId = transferInfo.TransferId;
            TransactionHash = transferInfo.TransactionHash;
            TransferStatus = transferInfo.TransferStatus;
            TransferStatusError = transferInfo.TransferStatusError;
            SourceAddresses = transferInfo.SourceAddresses;
            DestinationAddress = transferInfo.DestinationAddress;
            Amount = transferInfo.Amount;
            Currency = transferInfo.Currency;
        }

        public string TransferId { get => PartitionKey; set => PartitionKey = value;}
        public string TransactionHash { get => RowKey; set => RowKey = value; }
        [IgnoreProperty]
        public TransferStatus TransferStatus { get; set; }
        [IgnoreProperty]
        public TransferStatusError TransferStatusError { get; set; }
        [IgnoreProperty]
        public IEnumerable<ISourceAmount> SourceAddresses { get; set; }
        public string DestinationAddress { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public string STransferStatus
        {
            get => TransferStatus.ToString();
            set
            {
                TransferStatus result;
                if (!Enum.TryParse(value, out result))
                {
                    result = TransferStatus.Error;
                }
                TransferStatus = result;
            }
        }
        public string STransferStatusError
        {
            get => TransferStatusError.ToString();
            set
            {
                TransferStatusError result;
                if (!Enum.TryParse(value, out result))
                {
                    result = TransferStatusError.NotError;
                }
                TransferStatusError = result;
            }
        }

        public string SSourceAddresses
        {
            get => JsonConvert.SerializeObject(SourceAddresses);
            set
            {
                var result = new List<ISourceAmount>();
                try
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        SourceAddresses = result;
                        return;
                    }
                    result.AddRange(JsonConvert.DeserializeObject<List<SourceAmount>>(value));
                }
                catch 
                {
                    
                }

                SourceAddresses = result;
            }
        }
    }
}
