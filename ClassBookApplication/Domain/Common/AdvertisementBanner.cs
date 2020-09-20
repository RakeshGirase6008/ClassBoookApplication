namespace ClassBookApplication.Domain.Common
{
    public class AdvertisementBanner :BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool Active { get; set; }
    }
}
