using System.ComponentModel.DataAnnotations;
namespace ClassBookApplication.Models.PublicModel
{
    public class ForgotPasswordModel
    {
        [Display(Name = "Mail or UserName")]
        public string EmailId { get; set; }
    }
}
