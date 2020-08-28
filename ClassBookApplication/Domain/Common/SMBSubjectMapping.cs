namespace ClassBookApplication.Domain.Common
{
    public class SMBSubjectMapping : BaseEntity
    {
        public int SMBId { get; set; }
        public int SubjectId { get; set; }
        public bool Active { get; set; }
    }
}
