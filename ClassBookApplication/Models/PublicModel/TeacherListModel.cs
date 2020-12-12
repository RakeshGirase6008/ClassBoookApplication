using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClassBookApplication.Models.PublicModel
{
    public class TeacherListModel : CommonPageModel
    {
        public TeacherListModel()
        {
            States = new List<SelectListItem>();
            Cities = new List<SelectListItem>();
            BoardList = new List<SelectListItem>();
            MediumList = new List<SelectListItem>();
            StandardList = new List<SelectListItem>();
        }
        public IList<TeacherListingModel> TeacherModel { get; set; }

        // Validation Model
        [Display(Name = "Select State")]
        public int StateId { get; set; }
        public List<SelectListItem> States { get; set; }

        [Display(Name = "Select City")]
        public int CityId { get; set; }
        public List<SelectListItem> Cities { get; set; }

        [Display(Name = "Select Board")]
        public int BoardId { get; set; }
        public List<SelectListItem> BoardList { get; set; }

        [Display(Name = "Select Medium")]
        public int MediumId { get; set; }
        public List<SelectListItem> MediumList { get; set; }

        [Display(Name = "Select Standard")]
        public int StandardId { get; set; }
        public List<SelectListItem> StandardList { get; set; }
    }
}
