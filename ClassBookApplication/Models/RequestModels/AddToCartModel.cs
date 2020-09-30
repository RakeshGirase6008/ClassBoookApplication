using System.ComponentModel.DataAnnotations;

namespace ClassBookApplication.Models.RequestModels
{
    public class AddToCartModelClassTeacher
    {
        public int BoardId { get; set; }

        public int MediumId { get; set; }

        public int StandardId { get; set; }

        public int SubjectId { get; set; }

        public int CourseId { get; set; }

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
