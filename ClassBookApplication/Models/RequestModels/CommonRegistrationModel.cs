using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClassBookApplication.Models.RequestModels
{
    public class CommonRegistrationModel
    {
        public List<IFormFile> files { get; set; }
        public IFormFile file { get; set; }
        public string data { get; set; }
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public string FCMId { get; set; }
    }
}
