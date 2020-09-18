using System.Collections.Generic;

namespace ClassBookApplication.Models.ResponseModel
{
    public class TopicResponseModel
    {
        public TopicResponseModel()
        {
            subTopicResponseModel = new List<SubTopicResponseModel>();
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int SubTopicCount { get; set; }
        public List<SubTopicResponseModel> subTopicResponseModel { get; set; }
    }

    public class SubTopicResponseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string VideoLink { get; set; }
    }
}
