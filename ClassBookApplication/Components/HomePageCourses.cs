using ClassBookApplication.Service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
namespace ClassBookApplication.Components
{
    [ViewComponent(Name = "HomePageCourses")]
    public class HomePageCourses : ViewComponent
    {
        #region Fields

        private readonly ClassBookService _classBookService;

        #endregion

        #region Ctor
        public HomePageCourses(ClassBookService classBookService)
        {
            this._classBookService = classBookService;
        }

        #endregion
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var courseCategory = await _classBookService.GetAllCourses();
            return View(courseCategory);
        }
    }
}
