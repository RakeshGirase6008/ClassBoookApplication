using ClassBookApplication.Service;
using ClassBookApplication.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClassBookApplication.Components
{
    [ViewComponent(Name = "HomePageLatestClasses")]
    public class HomePageLatestClasses : ViewComponent
    {
        #region Fields

        private readonly ClassBookService _classBookService;

        #endregion

        #region Ctor
        public HomePageLatestClasses(ClassBookService classBookService)
        {
            this._classBookService = classBookService;
        }

        #endregion
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var courseCategory = await _classBookService.GetAllClasses((int)Module.Classes);
            return View(courseCategory);
        }
    }
}
