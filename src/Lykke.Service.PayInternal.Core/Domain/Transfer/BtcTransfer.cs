using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class BtcTransfer
    {
        public IEnumerable<AddressAmount> Sources { get; set; }
        public string DestAddress { get; set; }
        public int FeeRate { get; set; }
        public decimal FixedFee { get; set; }
    }
}
