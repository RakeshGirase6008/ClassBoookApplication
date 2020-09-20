namespace ClassBookApplication.Domain.Common
{
    public class CourseCategoryMapping : BaseEntity
    {
        public int CourseCategoryId { get; set; }
        public int ModuleId { get; set; }
        public int AssignToId { get; set; }
    }
}