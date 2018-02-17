using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.BtcTransfer
{
    public class BtcTransfer
    {
        public IEnumerable<SourceInfo> Sources { get; set; }
        public string DestAddress { get; set; }
        public int FeeRate { get; set; }
        public decimal FixedFee { get; set; }
    }
}
