namespace ClassBookApplication.Domain.Common
{
    public class CourseMapping: BaseEntity
    {
        public int EntityId { get; set; }
        public int ModuleId { get; set; }
        public int CourseId { get; set; }
        public decimal DistanceFees { get; set; }
        public decimal PhysicalFees { get; set; }
        public bool Active { get; set; }
    }
}
