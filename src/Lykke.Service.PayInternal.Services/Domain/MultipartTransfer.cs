using Lykke.Service.PayInternal.Core.Domain.Transfer;
using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class MultipartTransfer : IMultipartTransfer
    {
        public string TransferId { get; set; }
        public string PaymentRequestId { get; set; }
        public string MerchantId { get; set; }
        public DateTime CreationDate { get; set; }
        public string AssetId { get; set; }
        public int FeeRate { get; set; }
        public decimal FixedFee { get; set; }

        public List<TransferPart> Parts { get; set; }
    }
}
