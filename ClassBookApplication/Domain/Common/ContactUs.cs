namespace ClassBookApplication.Domain.Common
{
    public class ContactUs : BaseEntity
    {
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public string Message { get; set; }
        public string FromType { get; set; }
        public bool Active { get; set; }
    }
}
