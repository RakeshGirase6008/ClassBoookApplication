﻿using System;

namespace ClassBookApplication.Domain.Common
{
    public class Order : BaseEntity
    {
        public int UserId { get; set; }
        public DateTime? OrderDate { get; set; }
        public bool PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
    }
}