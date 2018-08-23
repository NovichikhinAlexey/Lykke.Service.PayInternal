﻿using System;

namespace Lykke.Service.PayInternal.Models.Transfers
{
    public class BtcTransferResponse
    {
        public string TransactionId { get; set; }

        public string Transaction { get; set; }

        public string Hash { get; set; }

        public Decimal Fee { get; set; }
    }
}
