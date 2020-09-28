namespace ClassBookApplication.Domain.Common
{
    public class OrderCartItems : BaseEntity
    {
        public int OrderId { get; set; }

        public int MappingId { get; set; }

        public string Type { get; set; }

        public decimal Amount { get; set; }
    }
}
