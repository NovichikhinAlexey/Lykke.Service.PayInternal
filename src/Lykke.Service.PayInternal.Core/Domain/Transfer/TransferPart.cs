using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class TransferPart
    {
        public List<AddressAmount> Sources { get; set; }
        public AddressAmount Destination { get; set; }
    }
}
