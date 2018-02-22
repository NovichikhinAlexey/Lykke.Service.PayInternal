using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models
{
    public class TransferRequest : ITransferRequest
    {
        public string TransferId { get; set; }
        public List<ITransactionRequest> TransactionRequests { get; set; }
        public TransferStatus TransferStatus { get; set; }
        public TransferStatusError TransferStatusError { get; set; }
        public DateTime CreateDate { get; set; }
        public string MerchantId { get; set; }

        public ITransferRequest DeepCopy()
        {
            var copy = new TransferRequest()
            {
                TransferId = TransferId,
                TransferStatus = TransferStatus,
                TransferStatusError = TransferStatusError,
                CreateDate = CreateDate,
                MerchantId = MerchantId
            };

            copy.TransactionRequests = new List<ITransactionRequest>();

            foreach (ITransactionRequest tran in TransactionRequests)
            {
                var tranCopy = new TransactionRequest()
                {
                    Amount = tran.Amount,
                    CountConfirm = tran.CountConfirm,
                    Currency = tran.Currency,
                    DestinationAddress = tran.DestinationAddress,
                    TransactionHash = tran.TransactionHash
                };

                tranCopy.SourceAmounts = (from s in tran.SourceAmounts
                                         select (IAddressAmount)(new AddressAmount()
                                         {
                                             Address = s.Address,
                                             Amount = s.Amount
                                         })).ToList();
                
                copy.TransactionRequests.Add(tranCopy);
            }

            return copy;
        }
    }
}
