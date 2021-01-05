using ClassBookApplication.Domain.Common;
using System.Collections.Generic;

namespace ClassBookApplication.Models.PublicModel
{
    public partial class SubjectViewModel
    {
        #region Ctor
        
        public SubjectViewModel()
        {
            SubjectList = new List<SubjectMappingModel>();
            TopicListModel = new List<TopicModel>();
        }

        #endregion
        
        #region Fields

        public List<SubjectMappingModel> SubjectList { get; set; } 
        public List<TopicModel> TopicListModel { get; set; }

        public string ModeuleName { get; set; }
        #endregion
    }

    public partial class SubjectMappingModel
    {
        public int SMBId { get; set; }

        public int SubjectId { get; set; }

        public string SubjectName { get; set; }

        public decimal DistanceFees { get; set; }

        public decimal PhysicalFees { get; set; }

        public bool Active { get; set; }
    }

    public partial class TopicModel
    {
        public int TopicId { get; set; }
        public int OrderItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
    }
}
