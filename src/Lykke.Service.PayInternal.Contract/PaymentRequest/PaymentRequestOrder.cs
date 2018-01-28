﻿using System;

namespace Lykke.Service.PayInternal.Contract.PaymentRequest
{
    public class PaymentRequestOrder
    {
        public string Id { get; set; }
        public double Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
