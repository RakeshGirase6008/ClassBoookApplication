namespace ClassBookApplication.Domain.Common
{
    public class MappingData : BaseEntity
    {
        public int ModuleId { get; set; }
        public int AssignToId { get; set; }
        public bool Active { get; set; }
    }
}
