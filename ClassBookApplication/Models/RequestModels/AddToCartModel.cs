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
        public int MappingId { get; set; }

        [Required]
        public string TypeOfMapping { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
