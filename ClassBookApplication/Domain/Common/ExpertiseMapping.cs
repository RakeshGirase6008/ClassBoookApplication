namespace ClassBookApplication.Domain.Common
{
    public class ExpertiseMapping : BaseEntity
    {
        public int EnityId { get; set; }
        public int ModuleId { get; set; }
        public int ExpertiseId { get; set; }
        public decimal DistanceFees { get; set; }
        public decimal PhysicalFees { get; set; }
        public bool Active { get; set; }
    }
}
