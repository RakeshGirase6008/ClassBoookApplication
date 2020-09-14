using System.Collections.Generic;

namespace ClassBookApplication.Models.ResponseModel
{
    public class DetailModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IntroductionURL { get; set; }
        public string CityName { get; set; }
    }

    public class ClassDetailModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IntroductionURL { get; set; }
        public string CityName { get; set; }
        public IList<ClassDetailModels> ClassDetailModels { get; set; }
    }
}
