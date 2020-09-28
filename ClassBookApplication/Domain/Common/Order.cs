using System;

namespace ClassBookApplication.Domain.Common
{
    public class Order : BaseEntity
    {
        public int EntityId { get; set; }
        public int ModuleId { get; set; }
        public string PaymentTye { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public bool PaymentStatus { get; set; }
        public decimal OurAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
