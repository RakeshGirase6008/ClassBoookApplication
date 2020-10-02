using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ClassBookApplication.Models.RequestModels
{
    public class TopicCommonRequestModel
    {
        public int Id { get; set; }
        public List<IFormFile> Files { get; set; }
        public List<IFormFile> Video { get; set; }
        public int OrderItemId { get; set; }
        public int TopicId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? DateOfUpload { get; set; }
        public DateTime? DateOfActivation { get; set; }
        public int UploadedByUserId { get; set; }
    }
}
