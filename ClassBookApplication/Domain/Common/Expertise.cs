namespace ClassBookApplication.Domain.Common
{
    public class Expertise :BaseEntity
    {
        public string Name { get; set; }
        public string ImageURL { get; set; }
        public bool Active { get; set; }
    }
}
