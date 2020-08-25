using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ClassBookApplication.Models.RequestModels
{
    public class CommonRegistrationModel
    {
        public List<IFormFile> files { get; set; }
        public string data { get; set; }
        public string DeviceId { get; set; }
        public string FCMId { get; set; }
    }
}
