using System.ComponentModel.DataAnnotations;

namespace ClassBookApplication.Models.ResponseModel
{
    public class SubjectRequestDetails
    {
        [Required]
        public int EntityId { get; set; }
        [Required]
        public int ModuleId { get; set; }
        [Required]
        public int BoardId { get; set; }
        [Required]
        public int MediumId { get; set; }
        [Required]
        public int StandardId { get; set; }
    }

    public class SubjectDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool InCart { get; set; }
        public decimal DistanceFees { get; set; }
        public decimal PhysicalFees { get; set; }
        public int SubjectMappingId { get; set; }
        public int OrderCartItemId { get; set; }
    }
}
