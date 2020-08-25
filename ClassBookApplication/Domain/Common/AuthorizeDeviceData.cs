namespace ClassBookApplication.Domain.Common
{
    public class AuthorizeDeviceData : BaseEntity
    {
        public int UserId { get; set; }
        public string DeviceId { get; set; }
    }
}
