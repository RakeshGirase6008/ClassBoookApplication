namespace ClassBookApplication.Domain.Common
{
    public class BoardMapping : BaseEntity
    {
        public int BoardId { get; set; }
        public int ModuleId { get; set; }
        public int AssignToId { get; set; }
        public bool Active { get; set; }
    }
}
