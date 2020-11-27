using ClassBookApplication.Service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClassBookApplication.Components
{
    [ViewComponent(Name = "RegistrationSide")]
    public class RegistrationSideComponent : ViewComponent
    {
        #region Fields

        private readonly ClassBookService _classBookService;

        #endregion

        #region Ctor
        public RegistrationSideComponent(ClassBookService classBookService)
        {
            this._classBookService = classBookService;
        }

        #endregion
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var courseCategory = await _classBookService.GetAllCourses();
            return View();
        }
    }
}
