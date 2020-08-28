using System.Collections.Generic;

namespace ClassBookApplication.Models.RequestModels
{
    public class MappingRequestModel
    {
        public List<StandardMediumBoard> StandardMediumBoard { get; set; }
        public string CourseCategoryIds { get; set; }
        public string SubjectSpecialityIds { get; set; }
    }
    public class StandardMediumBoard
    {
        public int BoardId { get; set; }
        public int MediumId { get; set; }
        public int StandardId { get; set; }
        public List<int> SubjectIds { get; set; }
    }
}


