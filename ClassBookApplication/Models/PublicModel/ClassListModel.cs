using System.Collections.Generic;

namespace ClassBookApplication.Models.PublicModel
{
    public class ClassListModel : CommonPageModel
    {
        public IList<ClassListingModel> ClassModel { get; set; }
    }
}