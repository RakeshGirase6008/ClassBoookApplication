namespace ClassBookApplication.Domain.Topics
{
    public class Topic : BaseEntity
    {
        public int OrderItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
    }
}
