using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClassBookApplication.Models.PublicModel
{
    public class CoursesListModel : CommonPageModel
    {
        public CoursesListModel()
        {
            States = new List<SelectListItem>();
            Cities = new List<SelectListItem>();
            CourseCategory = new List<SelectListItem>();
            TeacherList = new List<SelectListItem>();
            ClassesList = new List<SelectListItem>();
        }
        public IList<CoursesListingModel> CoursesModel { get; set; }

        // Validation Model
        [Display(Name = "Select State")]
        public int StateId { get; set; }
        public List<SelectListItem> States { get; set; }

        [Display(Name = "Select City")]
        public int CityId { get; set; }
        public List<SelectListItem> Cities { get; set; }

        [Display(Name = "Select Courses Category")]
        public int CourseCategoryId { get; set; }
        public List<SelectListItem> CourseCategory { get; set; }

        [Display(Name = "Select Teacher")]
        public int TeacherId { get; set; }
        public List<SelectListItem> TeacherList { get; set; }

        [Display(Name = "Select Classes")]
        public int ClassId { get; set; }
        public List<SelectListItem> ClassesList { get; set; }
    }
}
