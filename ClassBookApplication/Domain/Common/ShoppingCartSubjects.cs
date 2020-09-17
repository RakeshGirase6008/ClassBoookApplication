namespace ClassBookApplication.Domain.Common
{
    public class ShoppingCartSubjects :BaseEntity
    {
        public int SMBId { get; set; }
        public int UserId { get; set; }
        public int SubjectId { get; set; }
        public int LevelId { get; set; }
    }
}
