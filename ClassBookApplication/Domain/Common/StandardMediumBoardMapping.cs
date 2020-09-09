namespace ClassBookApplication.Domain.Common
{
    public class StandardMediumBoardMapping : BaseEntity
    { 
        public int UserId { get; set; }
        public int BoardId { get; set; }
        public int MediumId { get; set; }
        public int StandardId { get; set; }
        public bool Active { get; set; }
    }
}