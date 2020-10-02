namespace ClassBookApplication.Domain.Topics
{
    public class SubjectTopicMapping : BaseEntity
    {
        public int OrderItemId { get; set; }

        public int TopicId { get; set; }

        public bool Active { get; set; }
    }
}
