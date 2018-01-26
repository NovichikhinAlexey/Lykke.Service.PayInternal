using System;
using System.Collections.Generic;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class TransferDto : ITransferInfo
    {
        public TransferDto()
        {
            TransferId = Guid.NewGuid().ToString();
        }
        public string TransferId { get; set; }
        public string TransactionHash { get; set; }
        public TransferStatus TransferStatus { get; set; }
        public TransferStatusError TransferStatusError { get; set; }
        public IEnumerable<ISourceAmount> SourceAddresses { get; set; }
        public string DestinationAddress { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
