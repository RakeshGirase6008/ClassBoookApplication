namespace ClassBookApplication.Models.RequestModels
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
        public string FCMId { get; set; }
    }
}
