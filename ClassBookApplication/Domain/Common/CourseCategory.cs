namespace ClassBookApplication.Domain.Common
{
    public class CourseCategory : BaseEntity
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public bool Active { get; set; }
    }
}
