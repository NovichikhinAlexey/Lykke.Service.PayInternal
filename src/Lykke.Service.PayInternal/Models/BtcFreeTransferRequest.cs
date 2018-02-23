using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Models
{
    public class BtcFreeTransferRequest
    {
        [Required]
        public IEnumerable<BtcTransferSourceInfo> Sources { get; set; }

        [Required]
        public string DestAddress { get; set; }
    }

    public class BtcTransferSourceInfo
    {
        [Required]
        public string Address { get; set; }

        [Required]
        public decimal Amount { get; set; }
    }
}
