namespace ClassBookApplication.Domain.Common
{
    public class StandardMediumBoardMapping : BaseEntity
    { 
        public int EnityId { get; set; }
        public int ModuleId { get; set; }
        public int BoardId { get; set; }
        public int MediumId { get; set; }
        public int StandardId { get; set; }
        public bool Active { get; set; }
    }
}