﻿using System;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    public class PaymentRequestOrderModel
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
