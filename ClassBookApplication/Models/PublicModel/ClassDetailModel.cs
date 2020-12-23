using System.Collections.Generic;

namespace ClassBookApplication.Models.PublicModel
{
    public class ClassDetailModel
    {
        public ClassDetailModel()
        {
            Ratings = new Dictionary<int, int>();
        }
        public int Id { get; set; }
        public string ClassName { get; set; }
        public string IntroductionVideoUrl { get; set; }
        public string ProfilePhoto { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string Website { get; set; }
        public string EstablishmentDate { get; set; }
        public int TotalRating { get; set; }
        public int FavouriteCount { get; set; }
        public string Description { get; set; }
        public Dictionary<int, int> Ratings { get; set; }
        public List<StandardMediumBoardMappingData> StandardMediumBoardMapping { get; set; }
    }
    public class StandardMediumBoardMappingData
    {
        public int SmbId { get; set; }
        public int StandardId { get; set; }
        public string StandardName { get; set; }
        public int MediumId { get; set; }
        public string MediumName { get; set; }
        public int BoardId { get; set; }
        public string BoardName { get; set; }
    }
}
