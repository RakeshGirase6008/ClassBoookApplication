namespace ClassBookApplication.Domain.Topics
{
    public class TopicSubTopicMapping : BaseEntity
    {
        public int TopicId { get; set; }

        public int SubTopicId { get; set; }

        public bool Active { get; set; }
    }
}
