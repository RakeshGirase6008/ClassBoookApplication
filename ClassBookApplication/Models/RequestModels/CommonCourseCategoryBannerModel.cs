using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ClassBookApplication.Models.RequestModels
{
    public class CommonCourseCategoryBannerModel
    {
        public List<IFormFile> Files { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
