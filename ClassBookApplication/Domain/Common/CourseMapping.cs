namespace ClassBookApplication.Domain.Common
{
    public class CourseMapping: BaseEntity
    {
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public int ModuleId { get; set; }
        public bool Active { get; set; }
    }
}
