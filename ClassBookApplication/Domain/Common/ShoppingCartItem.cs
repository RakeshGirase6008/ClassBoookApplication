namespace ClassBookApplication.Domain.Common
{
    public class ShoppingCartItem :BaseEntity
    {
        public int SMBId { get; set; }
        public int SubjectId { get; set; }
        public int LevelId { get; set; }
    }
}
