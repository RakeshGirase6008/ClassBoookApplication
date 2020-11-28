using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClassBookApplication.Models.PublicModel
{
    public class SchoolRegisterModel
    {
        public SchoolRegisterModel()
        {
            States = new List<SelectListItem>();
            Cities = new List<SelectListItem>();
            Pincodes = new List<SelectListItem>();
        }

        [Display(Name = "School Name")]
        [Required(ErrorMessage = "School Name is required")]
        public string Name { get; set; }

        [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
        public string Email { get; set; }

        [Display(Name = "Mobile Number")]
        [Required(ErrorMessage = "Mobile no. is required")]
        [RegularExpression("^(?!0+$)(\\+\\d{1,3}[- ]?)?(?!0+$)\\d{10,15}$", ErrorMessage = "Please enter valid phone no.")]
        public string ContactNo { get; set; }

        [Display(Name = "Alternate Mobile Number")]
        [Required(ErrorMessage = "Mobile no. is required")]
        [RegularExpression("^(?!0+$)(\\+\\d{1,3}[- ]?)?(?!0+$)\\d{10,15}$", ErrorMessage = "Please enter valid phone no.")]
        public string AlternateContact { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EstablishmentDate { get; set; }

        [Required(ErrorMessage = "Address is Required")]
        public string Address { get; set; }

        public string Description { get; set; }

        [Display(Name = "School Registration Number")]
        public string RegistrationNo { get; set; }

        [Display(Name = "Teaching Experience")]
        public string TeachingExperience { get; set; }

        [Display(Name = "Select State")]
        [Required(ErrorMessage = "State is required")]
        public int StateId { get; set; }

        [Display(Name = "Select City")]
        public int CityId { get; set; }

        [Display(Name = "Select Pincode")]
        public int PincodeId { get; set; }

        [Display(Name = "Select Class Images")]
        public IFormFile ImageFile { get; set; }
        public string ImagePath { get; set; }
        public List<SelectListItem> States { get; set; }
        public List<SelectListItem> Cities { get; set; }
        public List<SelectListItem> Pincodes { get; set; }
    }
}
