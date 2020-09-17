namespace ClassBookApplication.Domain.Common
{
    public class OrderSubjects :BaseEntity
    {
        public int OrderId { get; set; }
        public int SMBId { get; set; }
        public int SubjectId { get; set; }
        public decimal Amount { get; set; }
    }
}