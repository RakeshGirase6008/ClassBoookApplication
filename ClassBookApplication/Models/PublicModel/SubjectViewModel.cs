using ClassBookApplication.Domain.Common;
using System.Collections.Generic;

namespace ClassBookApplication.Models.PublicModel
{
    public partial class SubjectViewModel
    {
        #region Ctor
        
        public SubjectViewModel()
        {
            SubjectList = new List<SubjectMapping>();
        }

        #endregion
        
        #region Fields

        public List<SubjectMapping> SubjectList { get; set; } 

        #endregion
    }
}
