using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClassBookApplication.Models.PublicModel
{
    public partial class SubtopicViewModel
    {
        public int TopicId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string VideoLink { get; set; }
        public DateTime? DateOfUpload { get; set; }
        public DateTime? DateOfActivation { get; set; }
        public int UploadedByUserId { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
    }
}
