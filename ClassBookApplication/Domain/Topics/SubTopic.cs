using System;

namespace ClassBookApplication.Domain.Topics
{
    public class SubTopic : BaseEntity
    {
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
