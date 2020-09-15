using System.Collections.Generic;

namespace ClassBookApplication.Models.ResponseModel
{
    public class CommonDetailModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IntroductionURL { get; set; }
        public string CityName { get; set; }
        public IList<BoardMediumStandardModel> BoardMediumStandardModel { get; set; }
    }

    public class BoardMediumStandardModel
    {
        public string Board { get; set; }
        public int BoardId { get; set; }
        public string Medium { get; set; }
        public int MediumId { get; set; }
        public string Standard { get; set; }
        public int StandardsId { get; set; }
    }
}
