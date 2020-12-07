using ClassBookApplication.Service;
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

        #region Method
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var getAllClasses = await _classBookService.GetAllClasses11();
            return View(getAllClasses);
        }

        #endregion
    }
}
