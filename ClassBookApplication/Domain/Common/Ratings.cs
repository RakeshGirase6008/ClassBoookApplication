namespace ClassBookApplication.Domain.Common
{
    public class Ratings : BaseEntity
    {
        public int UserId { get; set; }
        public string EntityName { get; set; }
        public float Rating { get; set; }
        public string Comment { get; set; }
        public bool AppropedSatus { get; set; }
        public bool Active { get; set; }
    }
}
