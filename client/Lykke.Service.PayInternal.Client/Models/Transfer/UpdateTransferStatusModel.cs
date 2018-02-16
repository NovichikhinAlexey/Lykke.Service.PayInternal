using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lykke.Service.PayInternal.Client.Models.Transfer
{
    public class UpdateTransferStatusModel
    {
        [Required]
        public string TransferId { get; set; }
        [Required]
        public string TransactionHash { get; set; }
        public TransferStatus TransferStatus { get; set; }
        public TransferStatusError TransferStatusError { get; set; }

       
    }

}
