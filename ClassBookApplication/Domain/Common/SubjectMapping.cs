namespace ClassBookApplication.Domain.Common
{
    public class SubjectMapping : BaseEntity
    {
        public int SMBId { get; set; }

        public int SubjectId { get; set; }

        public decimal DistanceFees { get; set; }

        public decimal PhysicalFees { get; set; }

        public bool Active { get; set; }
    }
}
