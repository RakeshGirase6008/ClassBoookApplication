using ClassBookApplication.Factory;
using Microsoft.AspNetCore.Mvc;

namespace ClassBookApplication.Controllers
{
    public class CommonController: Controller
    {
        #region Fields

        private readonly ClassBookModelFactory _classBookModelFactory;

        #endregion

        #region Ctor

        public CommonController(ClassBookModelFactory classBookModelFactory)
        {
            _classBookModelFactory = classBookModelFactory;
        }

        #endregion

        #region Utilities
        public IActionResult GetCities(int stateId)
        {
            return Json(_classBookModelFactory.PrepareCityDropDown(stateId));
        }

        public IActionResult GetPincodes(int cityId)
        {
            return Json(_classBookModelFactory.PreparePincodeDropDown(cityId));
        }

        #endregion
    }
}
