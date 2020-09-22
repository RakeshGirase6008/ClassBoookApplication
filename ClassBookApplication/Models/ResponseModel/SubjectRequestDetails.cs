using System.ComponentModel.DataAnnotations;

namespace ClassBookApplication.Models.ResponseModel
{
    public class SubjectRequestDetails
    {
        [Required]
        public int Id { get; set; }
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
        public int SMBMappingId { get; set; }
    }
}
