using System;
using System.Collections.Generic;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transfer;


namespace Lykke.Service.PayInternal.Models
{
    public class TransferRequestModel
    {
        public TransferRequestModel()
        {
            Amount = 0;
            Currency = LykkeConstants.BitcoinAssetId;
        }
        public string MerchantId { get; set; }
        public string DestinationAddress { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public virtual ITransferRequest ToTransferRequest()
        {
            return new TransferRequest
            {
                TransferId = Guid.NewGuid().ToString(),
                TransferStatus = TransferStatus.InProgress,
                TransferStatusError = TransferStatusError.NotError,
                CreateDate = DateTime.Now,
                MerchantId = MerchantId,
                TransactionRequests = new List<ITransactionRequest>()
                {
                    new TransactionRequest
                    {
                        DestinationAddress = DestinationAddress,
                        Amount = Amount,
                        Currency = Currency
                    }
                }
            };
        }
    }
}
