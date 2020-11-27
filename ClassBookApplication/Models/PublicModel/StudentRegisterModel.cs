using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace ClassBookApplication.Models.PublicModel
{
    public class StudentRegisterModel
    {
        public StudentRegisterModel()
        {
            States = new List<SelectListItem>();
            Cities = new List<SelectListItem>();
            Pincodes = new List<SelectListItem>();
            GenderList = new List<SelectListItem>();
            BoardList = new List<SelectListItem>();
            MediumList = new List<SelectListItem>();
            StandardList = new List<SelectListItem>();
        }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "FirstName is required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "LastName is required")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile no. is required")]
        [RegularExpression("^(?!0+$)(\\+\\d{1,3}[- ]?)?(?!0+$)\\d{10,15}$", ErrorMessage = "Please enter valid phone no.")]
        public string ContactNo { get; set; }

        [DataType(DataType.DateTime)]
        [Required(ErrorMessage = "Date of BirthDate is required")]
        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "Address is Required")]
        public string Address { get; set; }
        public string Description { get; set; }

        [Display(Name = "Select Gender")]
        [Required, Range(1, int.MaxValue, ErrorMessage = "Gender is required")]
        public int GenderId { get; set; }

        [Display(Name = "Select State")]
        [Required(ErrorMessage = "State is required")]
        public int StateId { get; set; }

        [Display(Name = "Select City")]
        public int CityId { get; set; }

        [Display(Name = "Select Pincode")]
        public int PincodeId { get; set; }

        [Display(Name = "Select Gender")]
        public string Gender { get; set; }

        [Display(Name = "Select Board")]
        public int BoardId { get; set; }

        [Display(Name = "Select Medium")]
        public int MediumId { get; set; }

        [Display(Name = "Select Standard")]
        public int StandardId { get; set; }

        [Display(Name = "Upload File")]
        public IFormFile ImageFile { get; set; }
        public string ImagePath { get; set; }
        public List<SelectListItem> States { get; set; }
        public List<SelectListItem> Cities { get; set; }
        public List<SelectListItem> Pincodes { get; set; }
        public List<SelectListItem> GenderList { get; set; }
        public List<SelectListItem> BoardList { get; set; }
        public List<SelectListItem> MediumList { get; set; }
        public List<SelectListItem> StandardList { get; set; }
    }
}
