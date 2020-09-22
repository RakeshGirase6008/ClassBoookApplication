using System.ComponentModel.DataAnnotations;

namespace ClassBookApplication.Models.RequestModels
{
    public class AddToCartModelClassTeacher
    {
        [Required]
        public int BoardId { get; set; }
        [Required]
        public int MediumId { get; set; }
        [Required]
        public int StandardId { get; set; }
        [Required]
        public int SubjectId { get; set; }
    }

    public class AddToCartModel
    {
        [Required]
        public int SMBMappingId { get; set; }

        [Required]
        public int SubjectId { get; set; }
    }
}
