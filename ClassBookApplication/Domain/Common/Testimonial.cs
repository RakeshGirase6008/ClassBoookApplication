namespace ClassBookApplication.Domain.Common
{
    public class Testimonial: BaseEntity
    {
        public string ClientName { get; set; }
        public string Position { get; set; }
        public int Rating { get; set; }
        public string PhotoUrl { get; set; }
        public string Descrption { get; set; }
        public bool Active { get; set; }
    }
}
