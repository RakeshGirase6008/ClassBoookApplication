﻿namespace ClassBookApplication.Domain.Common
{
    public class OrderCartItems : BaseEntity
    {
        public int OrderId { get; set; }
        public int MappingId { get; set; }
        public string TypeOfMapping { get; set; }
        public string Type { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal OurAmount { get; set; }
    }
}
