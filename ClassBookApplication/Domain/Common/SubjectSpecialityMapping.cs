namespace ClassBookApplication.Domain.Common
{
    public class SubjectSpecialityMapping : BaseEntity
    {
        public int SubjectSpecialityId { get; set; }

        public int ModuleId { get; set; }

        public int AssignToId { get; set; }
    }
}
