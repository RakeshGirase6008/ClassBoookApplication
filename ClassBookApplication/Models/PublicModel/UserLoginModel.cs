using System.ComponentModel.DataAnnotations;

namespace ClassBookApplication.Models.PublicModel
{
    public class UserLoginModel
    {
        [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        public string Password { get; set; }
        public string UserRole { get; set; }
    }
}
