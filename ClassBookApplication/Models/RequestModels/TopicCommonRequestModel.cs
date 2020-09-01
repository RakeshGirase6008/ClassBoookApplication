using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ClassBookApplication.Models.RequestModels
{
    public class TopicCommonRequestModel
    {
        public List<IFormFile> files { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string VideoLink { get; set; }
        public DateTime? DateOfUpload { get; set; }
        public DateTime? DateOfActivation { get; set; }
        public int UploadedByUserId { get; set; }
    }
}
