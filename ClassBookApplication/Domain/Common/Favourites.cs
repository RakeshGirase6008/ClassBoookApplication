namespace ClassBookApplication.Domain.Common
{
    public class Favourites: BaseEntity
    {
        public int UserId { get; set; }
        public int EntityId { get; set; }
        public string EntityName { get; set; }
    }
}
